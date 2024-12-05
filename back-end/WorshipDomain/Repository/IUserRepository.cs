using WorshipDomain.Core.Interfaces;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IUserRepository : IGenericRepository<int, User>
    {
    }
}
