using Dapper;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class UsuarioRepository : GenericRepository<int, Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(IContextRepository dbContext) : base(dbContext) { }

        public bool AutenticarUsuario(string email, string senha)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM Usuario
                WHERE Email = @Email
                AND Senha = @Senha;";

            return _dbConnection.QuerySingle<bool>(sql, new { Email = email, Senha = senha});
        }
    }
}
