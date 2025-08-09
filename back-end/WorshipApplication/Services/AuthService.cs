using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using WorshipDomain.Core.Entities;
using WorshipDomain.DTO.Auth;
using WorshipDomain.Entities;
using WorshipDomain.Repository;

namespace WorshipApplication.Services
{
    public class AuthService : ServiceBase<int, User, IAuthRepository>
    {
        private readonly IUserRepository _userRepository;
        private readonly RsaSecurityKey _rsaSecurityKey;
        private readonly WhatsAppService _whatsAppService;

        public AuthService(IAuthRepository repository, IUserRepository userRepository, RsaSecurityKey rsaSecurityKey, WhatsAppService whatsAppService) : base(repository)
        {
            _userRepository = userRepository;
            _rsaSecurityKey = rsaSecurityKey;
            _whatsAppService = whatsAppService;
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

        public async void RequestPasswordResetCode(string email)
        {
            var user = _userRepository.GetList(new { Email = email }).FirstOrDefault();
            if (user == null) return;

            var code = GenerateSecureCode();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("code", code)
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(_rsaSecurityKey, SecurityAlgorithms.RsaSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            user.ResetPasswordTokenCode = tokenHandler.WriteToken(token);
            _userRepository.Update(user);

            // Enviando o código via WhatsApp Api
            await _whatsAppService.SendVerificationCodeAsync($"55{user.PhoneNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "")}", code);
            Console.WriteLine($"Código de verificação: {code}");
        }

        public string VerifyResetCode(VerifyResetCodeDTO verifyResetCodeDTO)
        {
            try
            {
                var user = _userRepository.GetList(new { Email = verifyResetCodeDTO.Email }).FirstOrDefault();
                if (user == null || string.IsNullOrEmpty(user.ResetPasswordTokenCode))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _rsaSecurityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(user.ResetPasswordTokenCode, validations, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                    return null;
                
                var validCode = principal.FindFirst("code")?.Value;

                if (verifyResetCodeDTO.Code != validCode)
                    return null;

                return GeneratePasswordResetToken(user);
            }
            catch
            {
                return null;
            }
        }

        private string GeneratePasswordResetToken(User user)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("PasswordHash", user.Password)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(_rsaSecurityKey, SecurityAlgorithms.RsaSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ResetPassword(ResetPasswordDTO resetPasswordDTO, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _rsaSecurityKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validations, out var validatedToken);

                var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
                var tokenPasswordHash = principal.FindFirst("PasswordHash")?.Value;

                var user = _userRepository.GetList(new { Email = userEmail }).FirstOrDefault();
                if (user == null) return false;

                if (!ConstantTimeEquals(user.Password, tokenPasswordHash))
                    return false;

                user.Password = GenerateHashPassword(resetPasswordDTO.NewPassword);
                user.ResetPasswordTokenCode = null;
                _userRepository.Update(user);

                return true;
            }
            catch
            {
                return false;
            }
        }

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

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            var result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        private string GenerateSecureCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 900000 + 100000;
            return value.ToString();
        }
    }
}
