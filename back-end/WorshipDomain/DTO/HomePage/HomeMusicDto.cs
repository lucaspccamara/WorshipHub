namespace WorshipDomain.DTO.HomePage
{
    public class HomeMusicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Artist { get; set; }
        public string? NoteBase { get; set; }
        public string? NoteMode { get; set; }
        public decimal? Bpm { get; set; }
        public string? TimeSignature { get; set; }
        public int? DurationSeconds { get; set; }
        public string? ImageUrl { get; set; }
    }
}
