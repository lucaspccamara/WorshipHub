using WorshipDomain.Enums;

namespace WorshipDomain.DTO.User
{
    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public List<int> Position { get; set; }
        public bool Status { get; set; }
        public string AvatarUrl { get; set; }
    }
}
