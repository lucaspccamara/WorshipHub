using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class UserRepository : GenericRepository<int, User>, IUserRepository
    {
        public UserRepository(IContextRepository dbContext) : base(dbContext) { }
    }
}
