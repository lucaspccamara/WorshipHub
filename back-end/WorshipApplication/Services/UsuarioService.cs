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
            var usuarioExiste = _repository.GetList(new { Email = email }).Any();

            if (!usuarioExiste)
                return string.Empty;

            var senhaVerificada = VerificaSenha(email, senha);

            if (senhaVerificada)
                return _authService.GetAuthToken();

            return string.Empty;
        }

        private bool VerificaSenha(string email, string senha)
        {
            string senhaHash = ((IUsuarioRepository)_repository).GetSenhaHashPorEmail(email);

            return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
        }

        private static string GerarSenhaHash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);
    }
}
