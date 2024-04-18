using Microsoft.AspNetCore.Mvc;
using WorshipDomain.DTO;
using WorshipDomain.Repository;
using WorshipDomain.Services.Interfaces;

namespace WorshipApi.Controllers
{
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAuthService _authService;

        public UsuarioController(IUsuarioRepository usuarioRepository, IAuthService authService)
        {
            _usuarioRepository = usuarioRepository;
            _authService = authService;
        }

        [HttpPost("/login")]
        public ActionResult AutenticarUsuario([FromBody] UsuarioLoginDTO usuarioLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = _usuarioRepository.AutenticarUsuario(usuarioLogin.Email, usuarioLogin.Senha);

            if (result)
            {
                var token = _authService.GetAuthToken();
                return Ok(token);
            }
            return BadRequest("Login inválido");
        }
    }
}
