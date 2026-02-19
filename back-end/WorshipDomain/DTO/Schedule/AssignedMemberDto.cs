namespace WorshipDomain.DTO.Schedule
{
    public class AssignedMemberDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        // position as integer (use your Position enum values)
        public int Position { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        // availability for this schedule (true = available, false = not available, null = not responded)
        public bool? Available { get; set; }
    }
}