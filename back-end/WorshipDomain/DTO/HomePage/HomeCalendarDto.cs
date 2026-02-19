namespace WorshipDomain.DTO.HomePage
{
    public class HomeCalendarDto
    {
        // date formatted as string YYYY/MM/DD to match frontend quasar lib
        public string Date { get; set; } = "";
        public List<HomePositionDto> Positions { get; set; } = new();
        public List<HomeMusicDto> Musics { get; set; } = new();
    }
}
