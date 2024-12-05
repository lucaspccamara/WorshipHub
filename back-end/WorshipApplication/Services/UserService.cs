using WorshipDomain.Core.Entities;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class UserService : ServiceBase<int, User, IUserRepository>
    {
        public UserService(IUserRepository repository) : base(repository)
        {
        }
    }
}
