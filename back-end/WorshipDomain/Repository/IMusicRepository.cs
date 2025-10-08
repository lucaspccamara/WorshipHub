using WorshipDomain.Core.Entities;
using WorshipDomain.Core.Interfaces;
using WorshipDomain.DTO.Music;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IMusicRepository : IGenericRepository<int, Music>
    {
        ResultFilter<MusicOverviewDTO> GetListPaged(ApiRequest<MusicFilterDTO> request);
    }
}
