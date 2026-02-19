namespace WorshipDomain.DTO.Schedule
{
    public class SchedulesAssignmentsDetailsDto
    {
        public List<ScheduleInfoDto> Schedules { get; set; } = new();
        public Dictionary<int, List<AssignedMemberDto>> MembersByPosition { get; set; } = new();
        // scheduleId -> (positionValue -> userId)
        public Dictionary<int, Dictionary<int, int?>> CurrentAssignments { get; set; } = new();
    }
}
