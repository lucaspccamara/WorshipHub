using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.HomePage;
using WorshipDomain.Repository;
using WorshipDomain.Entities;

namespace WorshipApplication.Services
{
    public class HomeService : ServiceBase<int, Music, IHomeRepository>
    {
        public HomeService(IHomeRepository repository) : base(repository)
        {
        }

        public IEnumerable<HomeCalendarDto> GetCalendar(HomeCalendarFilterDto filter, int userId)
        {
            return _repository.GetCalendar(filter, userId);
        }
    }
}
