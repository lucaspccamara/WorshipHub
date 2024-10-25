using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WorshipDomain.Core.Entities;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class AuthService : ServiceBase<int, Usuario, IAuthRepository>
    {
        private readonly string _jwtPrivateKey;
        private readonly IUsuarioRepository _usuarioRepository;

        public AuthService(IAuthRepository repository, IConfiguration configuration, IUsuarioRepository usuarioRepository) : base(repository)
        {
            _jwtPrivateKey = configuration["JWT_PRIVATE_KEY"];
            _usuarioRepository = usuarioRepository;
        }

        public string AutenticarUsuario(string email, string senha)
        {
            var usuario = _usuarioRepository.GetList(new { Email = email }).FirstOrDefault();

            if (usuario == null)
                return string.Empty;

            var senhaVerificada = VerificaSenha(email, senha);

            if (senhaVerificada)
                return GetAuthToken(usuario);

            return string.Empty;
        }

        private bool VerificaSenha(string email, string senha)
        {
            string senhaHash = _repository.GetSenhaHashPorEmail(email);

            return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
        }

        private static string GerarSenhaHash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

        private string GetAuthToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var privateKeyBytes = Convert.FromBase64String(_jwtPrivateKey);
            var privateKeyPem = Encoding.UTF8.GetString(privateKeyBytes);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray());

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
                ]),
                Expires = DateTime.UtcNow.AddHours(6), // Tempo de expiração do token
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
