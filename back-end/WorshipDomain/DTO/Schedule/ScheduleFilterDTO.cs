using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Schedule
{
    public class ScheduleFilterDTO
    {
        [Required]
        public string StartDate { get; set; }

        [Required]
        public string EndDate { get; set; }

        [Required]
        public int? EventType { get; set; }
    }
}
