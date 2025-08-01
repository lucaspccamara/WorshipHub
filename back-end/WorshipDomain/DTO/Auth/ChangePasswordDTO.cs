using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Auth
{
    public class ChangePasswordDTO
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string ConfirmPassword { get; set; }
    }
}
