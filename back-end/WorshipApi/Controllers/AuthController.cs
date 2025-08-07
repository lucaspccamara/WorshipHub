using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using WorshipApplication.Services;
using WorshipDomain.DTO.Auth;

namespace WorshipApi.Controllers
{
    [Route("api/auths")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("BruteForceProtection")]
        public ActionResult AuthenticateUser(
            [FromServices] AuthService _authService,
            [FromBody] UserLoginDTO userLoginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string token = _authService.AuthenticateUser(userLoginDTO.Email, userLoginDTO.Password);

            if (string.IsNullOrEmpty(token))
                return Unauthorized("Login inválido");

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // true em produção (requer HTTPS)
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(6),
                Path = "/"
            });

            return Ok();
        }

        [HttpPost("request-password-reset-code")]
        [AllowAnonymous]
        [EnableRateLimiting("BruteForceProtection")]
        public ActionResult RequestPasswordReset(
            [FromServices] AuthService _authService,
            [FromServices] UserService _userService,
            [FromBody] RequestPasswordResetCodeDTO dto)
        {
            _authService.RequestPasswordResetCode(dto.Email);
            return Ok();
        }

        [HttpPost("verify-reset-code")]
        [AllowAnonymous]
        [EnableRateLimiting("BruteForceProtection")]
        public ActionResult VerifyResetCode(
            [FromServices] AuthService _authService,
            [FromServices] UserService _userService,
            [FromBody] VerifyResetCodeDTO verifyResetCodeDTO)
        {
            var token = _authService.VerifyResetCode(verifyResetCodeDTO);

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token inválido ou expirado.");

            Response.Cookies.Append("reset_password_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // true em produção (requer HTTPS)
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Path = "/"
            });

            return Ok();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("BruteForceProtection")]
        public ActionResult ResetPassword(
            [FromServices] AuthService _authService,
            [FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var token = Request.Cookies["reset_password_token"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token não encontrado ou expirado.");

            var result = _authService.ResetPassword(resetPasswordDTO, token);
            if (!result)
                return BadRequest("Token inválido ou expirado.");

            Response.Cookies.Delete("reset_password_token");

            return Ok("Senha alterada com sucesso.");
        }

        [HttpPost("change-password")]
        [Authorize]
        public ActionResult ChangePassword(
            [FromServices] AuthService _authService,
            [FromBody] ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _authService.ChangePassword(changePasswordDTO);
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public ActionResult GetCurrentUser()
        {
            var user = HttpContext.User;

            if (user == null || !user.Identity.IsAuthenticated)
                return Unauthorized();

            var name = user.FindFirst(ClaimTypes.Name)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(new
            {
                UserId = userId,
                Name = name,
                Email = email,
                Role = role
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return NoContent();
        }
    }
}
