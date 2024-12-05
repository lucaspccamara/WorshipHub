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
            builder.Where("eventType = @eventType", new { eventType = eventType.GetHashCode() });

            return _dbConnection.ExecuteScalar<int>(selector.RawSql, selector.Parameters) > 0;
        }

        public ResultFilter<ScheduleOverviewDTO> GetListPaged(ApiRequest<ScheduleFilterDTO> request)
        {
            var builder = new SqlBuilder();

            var selector = builder.AddTemplate($@"
SELECT SQL_CALC_FOUND_ROWS
    id, date, eventType, status
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
                builder.Where("eventType = @eventType", new { eventType = request.Filters.EventType });
            else
                builder.Where("eventType < 10");

            builder.OrderBy(request.GetSorting("date"));

            IEnumerable<ScheduleOverviewDTO> scheduleOverviewDTO;
            int count = 0;

            using (var multiReader = _dbConnection.QueryMultiple(selector.RawSql, selector.Parameters))
            {
                var scheduleList = multiReader.Read<(int Id, DateTime Date, int EventType, int Status)>();
                scheduleOverviewDTO = scheduleList.Select(schedule => new ScheduleOverviewDTO
                {
                    Id = schedule.Id,
                    Date = schedule.Date.ToString("dd/MM/yy"),
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
    }
}
