using System.ComponentModel.DataAnnotations.Schema;
using WorshipDomain.Enums;

namespace WorshipDomain.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("role")]
        public Role Role { get; set; }

        [Column("position")]
        public string Position { get; set;}
    }
}
