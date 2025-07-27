using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.User
{
    public class UserCreationDTO
    {
        [Required, MinLength(2)]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
