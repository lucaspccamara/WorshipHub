using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.User
{
    public class UserFilterDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public bool? Status { get; set; }
    }
}
