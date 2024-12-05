using WorshipDomain.Core.Entities;
using WorshipDomain.Core.Interfaces;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.Entities;
using WorshipDomain.Enums;

namespace WorshipDomain.Repository
{
    public interface IScheduleRepository : IGenericRepository<int, Schedule>
    {
        bool ExistSchedule(DateTime date, EventType eventType);
        ResultFilter<ScheduleOverviewDTO> GetListPaged(ApiRequest<ScheduleFilterDTO> request);
    }
}
