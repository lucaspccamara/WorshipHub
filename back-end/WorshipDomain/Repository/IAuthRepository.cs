using WorshipDomain.Core.Interfaces;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IAuthRepository : IGenericRepository<int, User>
    {
        string GetHashPasswordByEmail(string email);
    }
}
