using System.ComponentModel.DataAnnotations;
using WorshipDomain.Enums;

namespace WorshipDomain.DTO.Schedule
{
    public class ScheduleDTO
    {
        public int Id { get; set; }

        [Required]
        public string Date { get; set; }

        [Required]
        public EventType EventType { get; set; }

        [Required]
        public ScheduleStatus Status { get; set; }
    }
}
