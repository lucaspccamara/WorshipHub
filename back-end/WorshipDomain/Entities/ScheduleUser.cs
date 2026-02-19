using System.ComponentModel.DataAnnotations.Schema;

namespace WorshipDomain.Entities
{
    [Table("schedules_users")]
    public class ScheduleUser
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Column("position")]
        public int Position { get; set; }
    }
}
