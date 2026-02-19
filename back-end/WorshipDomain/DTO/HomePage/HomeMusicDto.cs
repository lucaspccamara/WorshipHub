namespace WorshipDomain.DTO.HomePage
{
    public class HomeMusicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Artist { get; set; }
        public string? Details { get; set; }
        public string? ImageUrl { get; set; }
    }
}
