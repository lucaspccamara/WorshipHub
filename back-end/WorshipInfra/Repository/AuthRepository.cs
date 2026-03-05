using Dapper;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class AuthRepository : GenericRepository<int, User>, IAuthRepository
    {
        public AuthRepository(IContextRepository dbContext) : base(dbContext) { }

        public string GetHashPasswordByEmail(string email)
        {
            const string Sql = @"
                SELECT password
                FROM users
                WHERE email = @Email
                AND status = 1;";

            return _dbConnection.QuerySingle<string>(Sql, new { Email = email});
        }
    }
}
