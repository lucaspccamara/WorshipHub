using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Escala
{
    public class EscalaOverviewDTO
    {
        [Required]
        public string Data { get; set; }

        [Required]
        public int Evento { get; set; }
    }
}
