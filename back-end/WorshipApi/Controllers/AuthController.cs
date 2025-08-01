using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorshipApplication.Services;
using WorshipDomain.DTO.Auth;

namespace WorshipApi.Controllers
{
    [Route("api/auths")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult AuthenticateUser(
            [FromServices] AuthService _authService,
            [FromBody] UserLoginDTO userLoginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string token = _authService.AuthenticateUser(userLoginDTO.Email, userLoginDTO.Password);

            if (!token.IsNullOrEmpty())
                return Ok(new { token = token });

            return Unauthorized("Login inválido");
        }

        [HttpPost("request-password-reset-code")]
        [AllowAnonymous]
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
        public ActionResult VerifyResetCode(
            [FromServices] AuthService _authService,
            [FromServices] UserService _userService,
            [FromBody] VerifyResetCodeDTO verifyResetCodeDTO)
        {
            var token = _authService.VerifyResetCode(verifyResetCodeDTO);

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token inválido ou expirado.");

            return Ok(token);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public ActionResult ResetPassword(
            [FromServices] AuthService _authService,
            [FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var result = _authService.ResetPassword(resetPasswordDTO);
            if (!result)
                return BadRequest("Token inválido ou expirado.");

            return Ok("Senha alterada com sucesso.");
        }

        [HttpPost("change-password")]
        public ActionResult ChangePassword(
            [FromServices] AuthService _authService,
            [FromBody] ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _authService.ChangePassword(changePasswordDTO);
            return NoContent();
        }
    }
}
