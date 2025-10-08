namespace WorshipDomain.DTO.Music
{
    public class MusicDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? NoteBase { get; set; }
        public string? NoteMode { get; set; }
        public decimal? Bpm { get; set; }
        public string? TimeSignature { get; set; }
        public string? Duration { get; set; }
        public string? VideoUrl { get; set; }
    }
}
