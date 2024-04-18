using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorshipApplication.Services;
using WorshipDomain.DTO;

namespace WorshipApi.Controllers
{
    public class UsuarioController : ControllerBase
    {
        public UsuarioController()
        {
        }

        [HttpPost("/login")]
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
