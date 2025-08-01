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

        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("role")]
        public Role Role { get; set; }

        [Column("position")]
        public string Position { get; set;}

        [Column("status")]
        public bool Status { get; set; }

        [Column("avatar_url")]
        public string AvatarUrl { get; set; }

        [Column("reset_password_token_code")]
        public string ResetPasswordTokenCode { get; set; }
    }
}
