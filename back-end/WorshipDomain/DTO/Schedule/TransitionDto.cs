namespace WorshipDomain.DTO.Schedule
{
    public class TransitionDto
    {
        public List<int> ScheduleIds { get; set; } = new();
        public int NewStatus { get; set; }
    }
}