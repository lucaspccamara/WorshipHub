using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorshipDomain.Core.Entities;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class AuthService : ServiceBase<int, Usuario>
    {
        private readonly string _authKey;
        public AuthService(IAuthRepository repository, IConfiguration configuration) : base(repository)
        {
            _authKey = configuration["AuthKey"];
        }

        public string AutenticarUsuario(string email, string senha)
        {
            var usuario = _repository.GetList(new { Email = email }).FirstOrDefault();

            if (usuario == null)
                return string.Empty;

            var senhaVerificada = VerificaSenha(email, senha);

            if (senhaVerificada)
                return GetAuthToken(usuario);

            return string.Empty;
        }

        private bool VerificaSenha(string email, string senha)
        {
            string senhaHash = ((IAuthRepository)_repository).GetSenhaHashPorEmail(email);

            return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
        }

        private static string GerarSenhaHash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

        private string GetAuthToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(6), // Tempo de expiração do token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
