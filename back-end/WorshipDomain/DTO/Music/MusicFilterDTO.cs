using System.ComponentModel.DataAnnotations;

namespace WorshipDomain.DTO.Music
{
    public class MusicFilterDTO
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Artist { get; set; }

        [Required]
        public string Album { get; set; }
    }
}
