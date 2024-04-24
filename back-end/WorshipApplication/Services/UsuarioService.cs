using WorshipDomain.Core.Entities;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class UsuarioService : ServiceBase<int, Usuario>
    {
        private readonly AuthService _authService;

        public UsuarioService(IUsuarioRepository repository, AuthService authService) : base(repository)
        {
            _authService = authService;
        }

        public string AutenticarUsuario(string email, string senha)
        {
            var result = ((IUsuarioRepository)_repository).AutenticarUsuario(email, senha);
            if (result)
                return _authService.GetAuthToken();

            return string.Empty;
        }
    }
}
