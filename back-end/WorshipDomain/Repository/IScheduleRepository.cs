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
        ScheduleRepertoireDto GetScheduleRepertoireDetails(int scheduleId);
        void SaveScheduleRepertoire(int scheduleId, IEnumerable<int> musicIds);
        SchedulesAssignmentsDetailsDto? GetSchedulesAssignmentsDetails(IEnumerable<int> scheduleIds);
        void SaveAssignments(int scheduleId, Dictionary<int, List<int>> assignments);
        void UpdateStatus(IEnumerable<int> scheduleIds, int newStatus);
        List<(int Id, string PhoneNumber, string Name, string FcmToken, int? Position)> GetUsersToNotifyForTransition(IEnumerable<int> scheduleIds, int newStatus);
        List<(int Id, string PhoneNumber, string Name, string FcmToken, int? Position)> GetAssignedUsers(int scheduleId);
    }
}
