using WorshipDomain.Entities;

namespace WorshipDomain.Repository
{
    public interface IUsuarioRepository
    {
        Guid CadastrarUsuario(Usuario usuario);
        bool AutenticarUsuario(string email, string senha);
    }
}
