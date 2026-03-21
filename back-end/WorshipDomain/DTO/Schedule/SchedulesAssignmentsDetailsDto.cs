namespace WorshipDomain.DTO.Schedule
{
    public class SchedulesAssignmentsDetailsDto
    {
        public List<ScheduleInfoDto> Schedules { get; set; } = new();
        public Dictionary<int, List<AssignedMemberDto>> MembersByPosition { get; set; } = new();
        // scheduleId -> (positionValue -> list of userIds)
        public Dictionary<int, Dictionary<int, List<int>>> CurrentAssignments { get; set; } = new();
        // scheduleId -> (userId -> bool?) — true=available, false=unavailable, null=no response
        public Dictionary<int, Dictionary<int, bool?>> AvailabilityBySchedule { get; set; } = new();
    }
}
