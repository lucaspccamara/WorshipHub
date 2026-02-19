namespace WorshipDomain.DTO.Schedule
{
    public class ScheduleAvailabilityDTO
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public int EventType { get; set; }
        public bool? Available { get; set; }
    }
}
