using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.User;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class AuthService : ServiceBase<int, User, IAuthRepository>
    {
        private readonly IUserRepository _userRepository;
        private readonly RsaSecurityKey _rsaSecurityKey;

        public AuthService(IAuthRepository repository, IUserRepository userRepository, RsaSecurityKey rsaSecurityKey) : base(repository)
        {
            _userRepository = userRepository;
            _rsaSecurityKey = rsaSecurityKey;
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

        public void ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            var user = _userRepository.GetList(new { Email = changePasswordDTO.Email }).FirstOrDefault();

            if (user == null) return;

            var passwordVerified = CheckPassword(changePasswordDTO.Email, changePasswordDTO.CurrentPassword);

            if (passwordVerified)
            {
                user.Password = GenerateHashPassword(changePasswordDTO.NewPassword);
                _userRepository.Update(user);
                return;
            }
        }

        private bool CheckPassword(string email, string password)
        {
            string hashPassword = _repository.GetHashPasswordByEmail(email);

            return BCrypt.Net.BCrypt.Verify(password, hashPassword);
        }

        public string GenerateHashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        private string GetAuthToken(User user)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                ]),
                Expires = DateTime.UtcNow.AddHours(6), // Token expiration time
                SigningCredentials = new SigningCredentials(_rsaSecurityKey, SecurityAlgorithms.RsaSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
