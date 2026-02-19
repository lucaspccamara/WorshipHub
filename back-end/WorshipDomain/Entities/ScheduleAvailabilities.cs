using System.ComponentModel.DataAnnotations.Schema;

namespace WorshipDomain.Entities
{
    [Table("schedules_availabilities")]
    public class ScheduleAvailabilities
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Column("available")]
        public bool? Available { get; set; }
    }
}
