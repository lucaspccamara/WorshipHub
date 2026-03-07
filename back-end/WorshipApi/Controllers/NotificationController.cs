using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorshipDomain.Repository;
using WorshipInfra.Repository;
using WorshipDomain.Entities;
using WorshipInfra.Database.Interfaces;

using WorshipApplication.Services;

namespace WorshipApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly FcmNotificationService _fcmService;

        public NotificationController(IUserRepository userRepository, FcmNotificationService fcmService)
        {
            _userRepository = userRepository;
            _fcmService = fcmService;
        }

        public class TokenPayload
        {
            public string FcmToken { get; set; }
        }

        [HttpPost("token")]
        public IActionResult SaveToken([FromBody] TokenPayload payload)
        {
            // Extrai o UserId do token JWT atual (ClaimTypes.NameIdentifier)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(payload.FcmToken))
                return BadRequest("O Token FCM é obrigatório.");

            var user = _userRepository.Get(userId);
            if (user == null)
                return NotFound("Usuário não encontrado.");

            // Atualiza apenas se for um token novo ou diferente, senão skip database call
            if (user.FcmToken != payload.FcmToken)
            {
                user.FcmToken = payload.FcmToken;
                _userRepository.Update(user);
            }

            return Ok();
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestPush()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var user = _userRepository.Get(userId);
            if (user == null || string.IsNullOrEmpty(user.FcmToken))
                return BadRequest("Usuário não encontrado ou sem Token FCM registrado.");

            await _fcmService.SendNotificationAsync(
                user.FcmToken, 
                "Teste de Push WorshipHub! 🚀", 
                "Se você está vendo isso, sua integração com Firebase PWA está funcionando 100%!",
                "/profile"
            );

            return Ok("Push enviado! Verifique seu navegador/dispositivo.");
        }
    }
}
