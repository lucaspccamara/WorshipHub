using System.ComponentModel.DataAnnotations.Schema;

namespace WorshipDomain.Entities
{
    [Table("musics")]
    public class Music
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("artist")]
        public string? Artist { get; set; }

        [Column("album")]
        public string? Album { get; set; }

        [Column("note_base")]
        public string? NoteBase { get; set; }

        [Column("note_mode")]
        public string? NoteMode { get; set; }

        [Column("bpm")]
        public decimal? Bpm { get; set; }

        [Column("time_signature")]
        public string? TimeSignature { get; set; }

        [Column("duration_seconds")]
        public int? DurationSeconds { get; set; }

        [Column("video_url")]
        public string? VideoUrl { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }
    }
}
