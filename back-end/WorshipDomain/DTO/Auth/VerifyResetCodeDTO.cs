using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Auth
{
    public class VerifyResetCodeDTO
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Code { get; set; }
    }
}
