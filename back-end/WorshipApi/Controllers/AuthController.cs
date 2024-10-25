using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorshipApplication.Services;
using WorshipDomain.DTO;

namespace WorshipApi.Controllers
{
    [Route("api/auths")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult AutenticarUsuario(
            [FromServices] AuthService _authService,
            [FromBody] UsuarioLoginDTO usuarioLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string token = _authService.AutenticarUsuario(usuarioLogin.Email, usuarioLogin.Senha);

            if (!token.IsNullOrEmpty())
                return Ok(new { Token = token });

            return Unauthorized("Login inválido");
        }
    }
}
