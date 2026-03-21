namespace WorshipDomain.DTO.Schedule
{
    public class ScheduleAssignmentsDto
    {
        // positionValue -> list of userIds
        public Dictionary<int, List<int>> Assignments { get; set; } = new();
    }
}