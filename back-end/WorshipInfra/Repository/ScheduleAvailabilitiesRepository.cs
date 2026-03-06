using Dapper;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class ScheduleAvailabilitiesRepository : GenericRepository<int, ScheduleAvailabilities>, IScheduleAvailabilitiesRepository
    {
        public ScheduleAvailabilitiesRepository(IContextRepository dbContext) : base(dbContext) { }

        public List<ScheduleAvailabilityDTO> GetPendingByUser(int userId)
        {
            const string sql = @"
                SELECT sa.id,
                       DATE_FORMAT(s.date, '%d/%m/%Y') as Date,
                       s.event_type as EventType,
                       sa.available as Available,
                       s.id as ScheduleId
                FROM schedules_availabilities sa
                JOIN schedules s ON s.id = sa.schedule_id
                WHERE sa.user_id = @UserId
                  AND s.status = 1
                ORDER BY s.date;";

            return _dbConnection.Query<ScheduleAvailabilityDTO>(sql, new { UserId = userId }).ToList();
        }

        public void UpdateAvailability(int id, bool available)
        {
            const string sql = @"UPDATE schedules_availabilities SET available = @Available WHERE id = @Id;";
            _dbConnection.Execute(sql, new { Available = available, Id = id });
        }

        public ScheduleAvailabilities? GetById(int id)
        {
            const string sql = @"SELECT id, user_id as UserId, schedule_id as ScheduleId, available FROM schedules_availabilities WHERE id = @Id LIMIT 1;";
            return _dbConnection.QuerySingleOrDefault<ScheduleAvailabilities>(sql, new { Id = id });
        }
    }
}
