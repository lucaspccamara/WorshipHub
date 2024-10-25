using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Escala
{
    public class EscalaOverviewDTO
    {
        public string Data { get; set; }
        public int Evento { get; set; }
        public bool Liberada { get; set; }
    }
}
