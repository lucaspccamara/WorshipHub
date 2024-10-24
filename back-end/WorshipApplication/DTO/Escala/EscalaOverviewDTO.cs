using System.ComponentModel.DataAnnotations;
using WorshipDomain.Enums;

namespace WorshipApplication.DTO.Escala
{
    public class EscalaOverviewDTO
    {
        [Required]
        public string Data { get; set; }

        [Required]
        public int Evento { get; set; }
    }
}
