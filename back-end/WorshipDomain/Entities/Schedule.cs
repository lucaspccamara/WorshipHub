using System.ComponentModel.DataAnnotations.Schema;
using WorshipDomain.Enums;

namespace WorshipDomain.Entities
{
    [Table("schedules")]
    public class Schedule
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("eventType")]
        public EventType EventType { get; set; }

        [Column("status")]
        public ScheduleStatus Status { get; set; }
    }
}
