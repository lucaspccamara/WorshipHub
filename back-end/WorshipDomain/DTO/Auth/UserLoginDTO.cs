using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Auth
{
    public class UserLoginDTO
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }
    }
}
