using System.ComponentModel.DataAnnotations.Schema;
using WorshipDomain.Enums;

namespace WorshipDomain.Entities
{
    [Table("usuarios")]
    public class Usuario
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("nome")]
        public string Nome { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("senha")]
        public string Senha { get; set; }

        [Column("perfil")]
        public Perfil Perfil { get; set; }

        [Column("cargo")]
        public string Cargo { get; set;}
    }
}
