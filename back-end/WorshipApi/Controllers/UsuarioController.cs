using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorshipApplication.DTO;
using WorshipApplication.Services;

namespace WorshipApi.Controllers
{
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult AutenticarUsuario(
            [FromServices] UsuarioService _usuarioService,
            [FromBody] UsuarioLoginDTO usuarioLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string token = _usuarioService.AutenticarUsuario(usuarioLogin.Email, usuarioLogin.Senha);

            if (!token.IsNullOrEmpty())
                return Ok(token);

            return BadRequest("Login inválido");
        }
    }
}
