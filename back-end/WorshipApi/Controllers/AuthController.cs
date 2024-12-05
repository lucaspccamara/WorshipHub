using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorshipApplication.Services;
using WorshipDomain.DTO.User;

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
            {
                return BadRequest(ModelState);
            }

            string token = _authService.AuthenticateUser(userLoginDTO.Email, userLoginDTO.Password);

            if (!token.IsNullOrEmpty())
                return Ok(new { Token = token });

            return Unauthorized("Login inválido");
        }
    }
}
