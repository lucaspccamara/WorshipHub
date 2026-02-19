namespace WorshipDomain.DTO.Schedule
{
    public class RepertoireDto
    {
        public List<RepertoireTrackDto> Tracks { get; set; } = new();
        public bool Release { get; set; } = false;
    }
}