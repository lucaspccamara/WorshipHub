using Dapper;
using WorshipDomain.DTO.HomePage;
using WorshipDomain.Entities;
using WorshipDomain.Enums;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    // TODO: Refatorar esse repository para a service de home consumir os repositories de schedule, music e user
    public class HomeRepository : GenericRepository<int, Music>, IHomeRepository
    {
        public HomeRepository(IContextRepository dbContext) : base(dbContext) { }

        public IEnumerable<HomeCalendarDto> GetCalendar(HomeCalendarFilterDto filter, int userId)
        {
            const string schedulesSql = @"
                SELECT id, date
                FROM schedules
                WHERE date between @StartDate and @EndDate
                AND status = @Status
                ORDER BY date;";

            var scheds = _dbConnection.Query(schedulesSql, new { StartDate = filter.StartDate, EndDate = filter.EndDate, Status = ScheduleStatus.Completed }).ToList();

            var result = new List<HomeCalendarDto>();

            foreach (var s in scheds)
            {
                var panel = new HomeCalendarDto
                {
                    Date = ((DateTime)s.date).ToString("yyyy/MM/dd"),
                    Positions = new List<HomePositionDto>(),
                    Musics = new List<HomeMusicDto>()
                };

                // load assignments (positions -> user)
                const string assignSql = @"
                    SELECT su.position, u.id AS user_id, u.name, u.avatar_url
                    FROM schedules_users su
                    LEFT JOIN users u ON u.id = su.user_id
                    WHERE su.schedule_id = @ScheduleId;";
                var assigns = _dbConnection.Query(assignSql, new { ScheduleId = (int)s.id }).Select(a => ((int)a.position, (int)a.user_id, (string)a.name, (string)a.avatar_url)).ToList();

                // build positions list
                foreach (var assign in assigns)
                {
                    panel.Positions.Add(new HomePositionDto
                    {
                        PositionId = assign.Item1,
                        Member = assign.Item3,
                        AvatarUrl = assign.Item4,
                        Highlight = assign.Item2 == userId ? true : false
                    });
                }

                // load musics for schedule (fallback to empty)
                const string musicsSql = @"
                    SELECT m.id, m.title, m.artist, 
                           COALESCE(sm.note_base, m.note_base) as note_base, 
                           m.note_mode as note_mode, 
                           COALESCE(sm.bpm, m.bpm) as bpm, 
                           m.time_signature, m.duration_seconds, m.image_url
                    FROM schedules_musics sm
                    JOIN musics m ON m.id = sm.music_id
                    WHERE sm.schedule_id = @ScheduleId
                    ORDER BY sm.`order` ASC, sm.id ASC;";

                var musRows = _dbConnection.Query(musicsSql, new { ScheduleId = (int)s.id }).ToList();
                panel.Musics = musRows.Select(m => new HomeMusicDto
                {
                    Id = (int)m.id,
                    Title = (string)m.title,
                    Artist = m.artist as string,
                    NoteBase = m.note_base as string,
                    NoteMode = m.note_mode as string,
                    Bpm = m.bpm as decimal?,
                    TimeSignature = m.time_signature as string,
                    DurationSeconds = m.duration_seconds as int?,
                    ImageUrl = m.image_url as string
                }).ToList();

                result.Add(panel);
            }

            return result;
        }
    }
}
