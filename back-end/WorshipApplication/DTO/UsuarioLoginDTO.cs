using System.ComponentModel.DataAnnotations;

namespace WorshipApplication.DTO
{
    public class UsuarioLoginDTO
    {
        [Required]
        public string? Email {  get; set; }

        [Required]
        [MinLength(4)]
        public string? Senha { get; set; }
    }
}
