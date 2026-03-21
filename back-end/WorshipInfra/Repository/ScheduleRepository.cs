using Dapper;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.Entities;
using WorshipDomain.Enums;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class ScheduleRepository : GenericRepository<int, Schedule>, IScheduleRepository
    {
        public ScheduleRepository(IContextRepository dbContext) : base(dbContext) { }

        public bool ExistSchedule(DateTime date, EventType eventType)
        {
            var builder = new SqlBuilder();
            var selector = builder.AddTemplate(@"
SELECT 1
FROM schedules
/**where**/
;");

            builder.Where("date = @date", new { date });
            builder.Where("event_type = @eventType", new { eventType = eventType.GetHashCode() });

            return _dbConnection.ExecuteScalar<int>(selector.RawSql, selector.Parameters) > 0;
        }

        public ResultFilter<ScheduleOverviewDTO> GetListPaged(ApiRequest<ScheduleFilterDTO> request)
        {
            var builder = new SqlBuilder();

            var selector = builder.AddTemplate($@"
SELECT SQL_CALC_FOUND_ROWS
    id, date, event_type, status
FROM schedules
/**where**/
/**orderby**/
LIMIT {(request.Page - 1) * request.Length}, {request.Length};

SELECT FOUND_ROWS() AS TotalRecords;");

            if (DateTime.TryParse(request.Filters.StartDate, out var starDate))
                builder.Where("date >= @starDate", new { starDate });

            if (DateTime.TryParse(request.Filters.EndDate, out var endDate))
                builder.Where("date <= @endDate", new { endDate });

            if (request.Filters.EventType.HasValue)
                builder.Where("event_type = @eventType", new { eventType = request.Filters.EventType });
            else
                builder.Where("event_type < 10");

            builder.OrderBy(request.GetSorting("date"));

            IEnumerable<ScheduleOverviewDTO> scheduleOverviewDTO;
            int count = 0;

            using (var multiReader = _dbConnection.QueryMultiple(selector.RawSql, selector.Parameters))
            {
                var scheduleList = multiReader.Read<(int Id, DateTime Date, int EventType, int Status)>();
                scheduleOverviewDTO = scheduleList.Select(schedule => new ScheduleOverviewDTO
                {
                    Id = schedule.Id,
                    Date = schedule.Date.ToString("dd/MM/yyyy"),
                    EventType = schedule.EventType,
                    Status = schedule.Status
                });

                count = multiReader.ReadSingle<int>();
            }

            var resultFilter = new ResultFilter<ScheduleOverviewDTO>
            {
                Data = scheduleOverviewDTO,
                TotalRecords = count
            };

            return resultFilter;
        }

        public ScheduleRepertoireDto GetScheduleRepertoireDetails(int scheduleId)
        {
            const string scheduleSql = @"
            SELECT id as ScheduleId, date as Date, event_type as EventType, status
            FROM schedules WHERE id = @Id LIMIT 1;";
            var scheduleRepertoireDto = _dbConnection.QuerySingleOrDefault<ScheduleRepertoireDto>(scheduleSql, new { Id = scheduleId });
            if (scheduleRepertoireDto == null) return null;

            // current assignments
            const string assignSql = @"
            SELECT position, user_id
            FROM schedules_users
            WHERE schedule_id = @Id;";
            var assigns = _dbConnection.Query(assignSql, new { Id = scheduleId }).ToList();
            foreach (var a in assigns)
            {
                int pos = (int)a.position;
                int? uid = a.user_id == null ? (int?)null : (int)a.user_id;
                if (!scheduleRepertoireDto.CurrentAssignments.ContainsKey(pos))
                    scheduleRepertoireDto.CurrentAssignments[pos] = new List<int>();
                if (uid.HasValue)
                    scheduleRepertoireDto.CurrentAssignments[pos].Add(uid.Value);
            }

            // flatten assigned members (join user info)
            if (assigns.Count > 0)
            {
                var userIds = assigns.Select(a => (int?)a.user_id).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToArray();
                if (userIds.Length > 0)
                {
                    const string usersSql = @"
                    SELECT id, name
                    FROM users
                    WHERE id IN @Ids;";
                    var users = _dbConnection.Query(usersSql, new { Ids = userIds }).ToDictionary(u => (int)u.id, u => (string)u.name);
                    foreach (var a in assigns)
                    {
                        if (a.user_id == null) continue;
                        int pos = (int)a.position;
                        int uid = (int)a.user_id;
                        if (users.TryGetValue(uid, out var name))
                        {
                            scheduleRepertoireDto.AssignedMembers.Add(new AssignedMemberSimpleDto { Id = uid, Name = name, Position = pos });
                        }
                    }
                }
            }

            // existing repertoire for schedule (ordered by position/index)
            const string repSql = @"
            SELECT 
                m.id AS Id,
                m.title AS Title,
                m.artist as Artist,
                m.album AS Album,
                m.note_base AS NoteBase,
                m.note_mode AS NoteMode,
                m.bpm AS Bpm,
                m.time_signature AS TimeSignature,
                m.duration_seconds AS DurationSeconds,
                m.video_url AS VideoUrl
            FROM schedules_musics sm
            JOIN musics m ON m.id = sm.music_id
            WHERE sm.schedule_id = @Id
            ORDER BY sm.`order` ASC, sm.id ASC;";
            scheduleRepertoireDto.Repertoire = _dbConnection.Query<RepertoireTrackDto>(repSql, new { Id = scheduleId }).ToList(); ;

            // provide available tracks catalog (active musics)
            // TODO - Criar coluna status para a tabela musics
            // TODO - Remover AvailableTracks de ScheduleRepertoireDto e mover essa query para a musics repository e aplicar paginação e filtros de busca (ex: título/artista)
            const string allTracksSql = @"
            SELECT id, title, artist, album, note_base, note_mode, bpm, time_signature, duration_seconds, video_url
            FROM musics
            WHERE 1 = 1
            ORDER BY title;";
            scheduleRepertoireDto.AvailableTracks = _dbConnection.Query<RepertoireTrackDto>(allTracksSql).ToList();

            return scheduleRepertoireDto;
        }

        public void SaveScheduleRepertoire(int scheduleId, IEnumerable<int> musicIds)
        {
            var openedHere = false;
            try
            {
                if (_dbConnection.State != System.Data.ConnectionState.Open)
                {
                    _dbConnection.Open();
                    openedHere = true;
                }

                using var tx = _dbConnection.BeginTransaction();
                try
                {
                    const string deleteSql = @"DELETE FROM schedules_musics WHERE schedule_id = @ScheduleId;";
                    _dbConnection.Execute(deleteSql, new { ScheduleId = scheduleId }, tx);

                    const string insertSql = @"
                    INSERT INTO schedules_musics (schedule_id, music_id, `order`)
                    VALUES (@ScheduleId, @MusicId, @Order);";

                    int order = 0;
                    foreach (var mid in musicIds ?? Enumerable.Empty<int>())
                    {
                        order++;
                        _dbConnection.Execute(insertSql, new { ScheduleId = scheduleId, MusicId = mid, Order = order }, tx);
                    }

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
            finally
            {
                if (openedHere)
                {
                    try { _dbConnection.Close(); } catch { }
                }
            }
        }

        public SchedulesAssignmentsDetailsDto GetSchedulesAssignmentsDetails(IEnumerable<int> scheduleIds)
        {
            if (scheduleIds == null) throw new ArgumentNullException(nameof(scheduleIds));
            var ids = scheduleIds.ToArray();
            if (ids.Length == 0) return new SchedulesAssignmentsDetailsDto();

            var dto = new SchedulesAssignmentsDetailsDto();

            // 1) schedules basic info
            const string schedulesSql = @"
                SELECT id, date, event_type, status
                FROM schedules
                WHERE id IN @Ids
                ORDER BY date;";
            var schedules = _dbConnection.Query(schedulesSql, new { Ids = ids }).ToList();
            dto.Schedules = schedules.Select(s => new ScheduleInfoDto
            {
                ScheduleId = (int)s.id,
                Date = (DateTime)s.date,
                EventType = (int)s.event_type,
                Status = (int)s.status
            }).ToList();

            // 2) members grouped by position (all active users)
            const string usersSql = @"
                SELECT id, name, avatar_url, position
                FROM users
                WHERE status = 1
                ORDER BY name;";
            var users = _dbConnection.Query(usersSql).ToList();

            var membersByPos = new Dictionary<int, List<AssignedMemberDto>>();
            foreach (var u in users)
            {
                var posField = (u.position ?? "").ToString();
                var parts = posField.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    if (!int.TryParse(p.Trim(), out int posVal)) continue;
                    if (!membersByPos.ContainsKey(posVal)) membersByPos[posVal] = new List<AssignedMemberDto>();

                    membersByPos[posVal].Add(new AssignedMemberDto
                    {
                        Id = (int)u.id,
                        Name = (string)u.name,
                        AvatarUrl = u.avatar_url as string,
                        Position = posVal
                    });
                }
            }

            // 3) availability rows for the requested schedules
            const string availSql = @"
                SELECT schedule_id, user_id, available
                FROM schedules_availabilities
                WHERE schedule_id IN @Ids;";
            var avRows = _dbConnection.Query(availSql, new { Ids = ids }).ToList();
            var availability = new Dictionary<int, Dictionary<int, bool?>>();
            foreach (var r in avRows)
            {
                int scheduleId = (int)r.schedule_id;
                int userId = (int)r.user_id;
                bool? avail = r.available as bool?;
                if (!availability.ContainsKey(scheduleId)) availability[scheduleId] = new Dictionary<int, bool?>();
                availability[scheduleId][userId] = avail;
            }

            // Inject availability into membersByPos per schedule is not needed globally;
            // consumers expect members list per position, availability is separated per schedule.
            dto.MembersByPosition = membersByPos;
            dto.AvailabilityBySchedule = availability;

            // 4) current assignments for the requested schedules
            const string assignSql = @"
                SELECT schedule_id, position, user_id
                FROM schedules_users
                WHERE schedule_id IN @Ids;";
            var assignRows = _dbConnection.Query(assignSql, new { Ids = ids }).ToList();
            var assignments = new Dictionary<int, Dictionary<int, List<int>>>();
            foreach (var r in assignRows)
            {
                int scheduleId = (int)r.schedule_id;
                int pos = (int)r.position;
                int? uid = r.user_id == null ? (int?)null : (int)r.user_id;
                if (!assignments.ContainsKey(scheduleId)) assignments[scheduleId] = new Dictionary<int, List<int>>();
                if (!assignments[scheduleId].ContainsKey(pos)) assignments[scheduleId][pos] = new List<int>();
                if (uid.HasValue) assignments[scheduleId][pos].Add(uid.Value);
            }

            // Ensure every requested schedule has an entry (even empty)
            foreach (var id in ids)
            {
                if (!assignments.ContainsKey(id))
                {
                    assignments[id] = new Dictionary<int, List<int>>();
                }
            }

            dto.CurrentAssignments = assignments;

            return dto;
        }

        public void SaveAssignments(int scheduleId, Dictionary<int, List<int>> assignments)
        {
            _dbConnection.Open();
            using var tx = _dbConnection.BeginTransaction();
            try
            {
                // delete previous schedule assignments
                const string deleteSql = @"DELETE FROM schedules_users WHERE schedule_id = @ScheduleId;";
                _dbConnection.Execute(deleteSql, new { ScheduleId = scheduleId }, tx);

                const string insertSql = @"
                    INSERT INTO schedules_users (user_id, schedule_id, position)
                    VALUES (@UserId, @ScheduleId, @Position);";

                foreach (var kv in assignments)
                {
                    var position = kv.Key;
                    var userIds = kv.Value;
                    if (userIds == null) continue;
                    foreach (var userId in userIds)
                    {
                        _dbConnection.Execute(insertSql, new { UserId = userId, ScheduleId = scheduleId, Position = position }, tx);
                    }
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
            finally
            {
                _dbConnection.Close();
            }
        }

        public void UpdateStatus(IEnumerable<int> scheduleIds, int newStatus)
        {
            const string sql = @"UPDATE schedules SET status = @Status WHERE id IN @Ids;";
            _dbConnection.Execute(sql, new { Status = newStatus, Ids = scheduleIds.ToArray() });
        }

        public List<(int Id, string PhoneNumber, string Name, string FcmToken, int? Position)> GetUsersToNotifyForTransition(IEnumerable<int> scheduleIds, int newStatus)
        {
            const string sql = @"
                SELECT DISTINCT u.id, u.phone_number, u.name, u.fcm_token, su.position
                FROM schedules_users su
                JOIN users u ON u.id = su.user_id
                WHERE su.schedule_id IN @Ids;";

            var rows = _dbConnection.Query(sql, new { Ids = scheduleIds.ToArray() }).ToList();
            var list = rows.Select(r => ((int)r.id, (string)r.phone_number, (string)r.name, (string)r.fcm_token, (int?)r.position)).ToList();
            if (list.Count > 0) return list;

            const string allSql = @"SELECT id, phone_number, name, fcm_token, NULL as position FROM users WHERE status = 1;";
            var all = _dbConnection.Query(allSql).Select(r => ((int)r.id, (string)r.phone_number, (string)r.name, (string)r.fcm_token, (int?)null)).ToList();
            return all;
        }

        public List<(int Id, string PhoneNumber, string Name, string FcmToken, int? Position)> GetAssignedUsers(int scheduleId)
        {
            const string sql = @"
                SELECT DISTINCT u.id, u.phone_number, u.name, u.fcm_token, su.position
                FROM schedules_users su
                JOIN users u ON u.id = su.user_id
                WHERE su.schedule_id = @ScheduleId;";

            var rows = _dbConnection.Query(sql, new { ScheduleId = scheduleId }).ToList();
            return rows.Select(r => ((int)r.id, (string)r.phone_number, (string)r.name, (string)r.fcm_token, (int?)r.position)).ToList();
        }
    }
}
