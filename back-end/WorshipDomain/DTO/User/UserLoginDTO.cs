using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.User
{
    public class UserLoginDTO
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        [MinLength(4)]
        public string? Password { get; set; }
    }
}
