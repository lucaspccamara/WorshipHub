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

        [Column("tone")]
        public string? Tone { get; set; }
    }
}
