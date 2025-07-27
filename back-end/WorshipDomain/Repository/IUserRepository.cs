using WorshipDomain.Core.Entities;
using WorshipDomain.Core.Interfaces;
using WorshipDomain.DTO.User;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IUserRepository : IGenericRepository<int, User>
    {
        ResultFilter<UserOverviewDTO> GetListPaged(ApiRequest<UserFilterDTO> request);
        void Create(UserCreationDTO userCreationDTO);
    }
}
