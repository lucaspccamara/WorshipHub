using System.ComponentModel.DataAnnotations.Schema;
using WorshipDomain.Enums;

namespace WorshipDomain.Entities
{
    [Table("escalas")]
    public class Escala
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("data")]
        public DateTime Data { get; set; }

        [Column("liberada")]
        public bool Liberada { get; set; }
    }
}
