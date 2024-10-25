using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Escala
{
    public class EscalaFilterDTO
    {
        [Required]
        public string DataInicio { get; set; }

        [Required]
        public string DataFim { get; set; }

        [Required]
        public int Evento { get; set; }
    }
}
