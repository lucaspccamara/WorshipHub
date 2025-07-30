using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.User
{
    public class UserCreationDTO
    {
        [Required, MinLength(2)]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public List<int> Position { get; set; }

        public string Password { get; set; }
    }
}
