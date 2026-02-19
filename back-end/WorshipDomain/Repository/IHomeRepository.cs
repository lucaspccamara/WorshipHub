using WorshipDomain.Core.Interfaces;
using WorshipDomain.DTO.HomePage;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public partial interface IHomeRepository : IGenericRepository<int, Music>
    {
        IEnumerable<HomeCalendarDto> GetCalendar(HomeCalendarFilterDto filer, int userId);
    }
}
