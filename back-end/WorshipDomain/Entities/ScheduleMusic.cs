using System.ComponentModel.DataAnnotations.Schema;

namespace WorshipDomain.Entities
{
    [Table("schedules_musics")]
    public class ScheduleMusic
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("music_id")]
        public int MusicId { get; set; }

        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Column("order")]
        public int Order { get; set; }
    }
}
