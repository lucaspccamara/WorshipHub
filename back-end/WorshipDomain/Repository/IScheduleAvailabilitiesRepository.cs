using WorshipDomain.Core.Interfaces;
using WorshipDomain.DTO.Schedule;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IScheduleAvailabilitiesRepository : IGenericRepository<int, ScheduleAvailabilities>
    {
        List<ScheduleAvailabilityDTO> GetPendingByUser(int userId);
        void UpdateAvailability(int id, bool available);
        ScheduleAvailabilities? GetById(int id);
    }
}
