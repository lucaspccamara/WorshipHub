namespace WorshipDomain.DTO.Schedule
{
    public class ScheduleAssignmentsDto
    {
        // positionValue -> userId
        public Dictionary<int, int?> Assignments { get; set; } = new();
    }
}