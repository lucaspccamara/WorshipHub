using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using WorshipDomain.Core.Entities;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class AuthService : ServiceBase<int, User, IAuthRepository>
    {
        private readonly string _jwtPrivateKey;
        private readonly IUserRepository _userRepository;

        public AuthService(IAuthRepository repository, IConfiguration configuration, IUserRepository userRepository) : base(repository)
        {
            _jwtPrivateKey = configuration["JWT_PRIVATE_KEY"];
            _userRepository = userRepository;
        }

        public string AuthenticateUser(string email, string password)
        {
            var user = _userRepository.GetList(new { Email = email }).FirstOrDefault();

            if (user == null)
                return string.Empty;

            var passwordVerified = CheckPassword(email, password);

            if (passwordVerified)
                return GetAuthToken(user);

            return string.Empty;
        }

        private bool CheckPassword(string email, string password)
        {
            string hashPassword = _repository.GetHashPasswordByEmail(email);

            return BCrypt.Net.BCrypt.Verify(password, hashPassword);
        }

        private static string GenerateHashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        private string GetAuthToken(User user)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(_jwtPrivateKey.ToCharArray()); // Import private key

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                ]),
                Expires = DateTime.UtcNow.AddHours(6), // Token expiration time
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
