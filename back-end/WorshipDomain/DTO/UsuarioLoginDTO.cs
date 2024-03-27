using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorshipDomain.DTO
{
    public class UsuarioLoginDTO
    {
        [Required]
        public string? Usuario {  get; set; }

        [Required]
        [MinLength(4)]
        public string? Senha { get; set; }
    }
}
