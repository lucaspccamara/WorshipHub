using Dapper;
using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class UsuarioRepository : GenericRepository, IUsuarioRepository
    {
        public UsuarioRepository(IDbContext dbContext) : base(dbContext) { }

        public bool AutenticarUsuario(string email, string senha)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM whdatabase.usuario
                WHERE Email = @Email
                AND Senha = @Senha;";

            return _dbConnection.QuerySingle<bool>(sql, new { Email = email, Senha = senha});
        }

        public Guid CadastrarUsuario(Usuario usuario)
        {
            throw new NotImplementedException();
        }
    }
}
