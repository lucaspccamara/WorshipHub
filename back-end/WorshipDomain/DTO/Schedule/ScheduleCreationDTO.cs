using System.ComponentModel.DataAnnotations;
using WorshipDomain.Enums;

namespace WorshipDomain.DTO.Schedule
{
    public class ScheduleCreationDTO
    {
        [Required]
        public string Date { get; set; }

        [Required]
        public EventType EventType { get; set; }
    }
}
