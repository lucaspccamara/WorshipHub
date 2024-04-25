using WorshipDomain.Entities;
using WorshipDomain.Repository;
using WorshipInfra.Database;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Repository
{
    public class UsuarioRepository : GenericRepository<int, Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(IContextRepository dbContext) : base(dbContext) { }
    }
}
