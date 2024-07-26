using System.ComponentModel.DataAnnotations.Schema;
using WorshipDomain.Enums;

namespace WorshipDomain.Entities
{
    [Table("musicas")]
    public class Musica
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("titulo")]
        public string Titulo { get; set; }

        [Column("artista")]
        public string? Artista { get; set; }

        [Column("album")]
        public string? Album { get; set; }

        [Column("tom")]
        public string? Tom { get; set; }
    }
}
