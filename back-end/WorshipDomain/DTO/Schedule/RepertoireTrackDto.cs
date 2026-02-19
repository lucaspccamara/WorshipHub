namespace WorshipDomain.DTO.Schedule
{
    public class RepertoireTrackDto
    {
        public int Id { get; set; }                 // music id (int)
        public string Title { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? NoteBase { get; set; }
        public string? NoteMode { get; set; }
        public decimal? Bpm { get; set; }
        public string? TimeSignature { get; set; }
        public int? DurationSeconds { get; set; }
        public string? VideoUrl { get; set; }
    }
}