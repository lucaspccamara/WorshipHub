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
                        Icon = "/logo-512x512.png",
                        Badge = "/logo-transparent.svg",
                        Tag = "worship-push",
                        Renotify = true
                    }
                }
            };

            try
            {
                if (FirebaseMessaging.DefaultInstance == null)
                {
                    _logger.LogError("Falha ao disparar Push: Firebase Messaging não foi inicializado (DefaultInstance é nulo). Verifique se o arquivo firebase-service-account.json está presente e acessível.");
                    return;
                }

                // Dispara o Push pro Firebase
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao disparar FCM Push Notification via FirebaseAdmin.");
            }
        }
    }
}
