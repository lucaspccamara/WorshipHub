using Dapper;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class AuthRepository : GenericRepository<int, Usuario>, IAuthRepository
    {
        public AuthRepository(IContextRepository dbContext) : base(dbContext) { }

        public string GetSenhaHashPorEmail(string email)
        {
            const string Sql = @"
                SELECT senha
                FROM usuarios
                WHERE email = @Email;";

            return _dbConnection.QuerySingle<string>(Sql, new { Email = email});
        }
    }
}
