using WorshipDomain.Core.Interfaces;
using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IUsuarioRepository : IGenericRepository<int, Usuario>
    {
        string GetSenhaHashPorEmail(string email);
    }
}
