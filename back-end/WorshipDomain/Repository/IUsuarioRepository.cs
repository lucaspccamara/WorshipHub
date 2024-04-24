using WorshipDomain.Core.Interfaces;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IUsuarioRepository : IGenericRepository<int, Usuario>
    {
        bool AutenticarUsuario(string email, string senha);
    }
}
