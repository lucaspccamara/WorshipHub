using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly AuthService _authService;

        public UsuarioService(IUsuarioRepository usuarioRepository, AuthService authService)
        {
            _usuarioRepository = usuarioRepository;
            _authService = authService;
        }

        public string AutenticarUsuario(string email, string senha)
        {
            var result = _usuarioRepository.AutenticarUsuario(email, senha);
            
            if (result)
                return _authService.GetAuthToken();

            return string.Empty;
        }
    }
}
