namespace WorshipDomain.DTO.Schedule
{
    public class ScheduleRepertoireDto
    {
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public int EventType { get; set; }
        public int Status { get; set; }

        // assignments saved in DB: position -> list of userIds
        public Dictionary<int, List<int>> CurrentAssignments { get; set; } = new();

        // members currently assigned (flatten)
        public List<AssignedMemberSimpleDto> AssignedMembers { get; set; } = new();

        // repertoire currently saved (order preserved)
        public List<RepertoireTrackDto> Repertoire { get; set; } = new();

        // candidate tracks to choose from (catalog)
        public List<RepertoireTrackDto> AvailableTracks { get; set; } = new();
    }
}