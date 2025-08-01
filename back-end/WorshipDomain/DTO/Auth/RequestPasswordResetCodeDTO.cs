using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Auth
{
    public class RequestPasswordResetCodeDTO
    {
        [Required]
        public string Email { get; set; }
    }
}
