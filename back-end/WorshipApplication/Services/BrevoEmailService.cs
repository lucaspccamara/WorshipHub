using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorshipDomain.Configurations;
using WorshipDomain.Interfaces;

namespace WorshipApplication.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly BrevoSettings _settings;

        public BrevoEmailService(HttpClient httpClient, IOptions<BrevoSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string userName, string code)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey)) return false;

            var payload = new
            {
                sender = new { name = _settings.SenderName, email = _settings.SenderEmail },
                to = new[] { new { email = toEmail, name = userName } },
                subject = "Recuperação de Senha - WorshipHub",
                htmlContent = $@"
                    <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #1976d2; text-align: center;'>WorshipHub</h2>
                        <p>Olá, <strong>{userName}</strong>!</p>
                        <p>Você solicitou a recuperação de sua senha. Use o código abaixo para validar a alteração:</p>
                        <div style='background: #f1f2f3; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #333; border-radius: 5px; margin: 20px 0;'>
                            {code}
                        </div>
                        <p>Este código expira em 10 minutos.</p>
                        <p style='font-size: 12px; color: #777; margin-top: 30px; border-top: 1px solid #eee; padding-top: 10px;'>
                            Se você não solicitou esta alteração, ignore este e-mail.
                        </p>
                    </div>"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", _settings.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            
            return response.IsSuccessStatusCode;
        }
    }
}
