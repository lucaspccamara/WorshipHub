using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace WorshipApplication.Services
{
    public class FcmNotificationService
    {
        private readonly ILogger<FcmNotificationService> _logger;

        public FcmNotificationService(ILogger<FcmNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendNotificationAsync(string fcmToken, string title, string body, string url = "/")
        {
            if (string.IsNullOrEmpty(fcmToken))
            {
                _logger.LogWarning("Tentativa de envio de Push falhou: usuário não possui FcmToken preenchido.");
                return;
            }

            var message = new Message()
            {
                Token = fcmToken,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>()
                {
                    { "url", url }
                },
                Webpush = new WebpushConfig()
                {
                    Notification = new WebpushNotification()
                    {
                        Icon = "/pwa-192x192.png",
                        Badge = "/vite.svg",
                        Tag = "worship-push",
                        Renotify = true
                    }
                }
            };

            _logger.LogInformation("Enviando Push para o token iniciado em: {TokenPrefix}...", fcmToken.Substring(0, Math.Min(10, fcmToken.Length)));

            try
            {
                // Dispara o Push pro Firebase
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("FCM Push enviado com sucesso. Firebase message ID: {Id}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao disparar FCM Push Notification via FirebaseAdmin.");
            }
        }
    }
}
