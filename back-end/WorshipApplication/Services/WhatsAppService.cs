using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WorshipApplication.Services
{
    public class WhatsAppService
    {
        private readonly string _token;
        private readonly string _phoneNumberId;
        
        public WhatsAppService(IConfiguration configuration)
        {
            _token = configuration["WhatsApp:ApiToken"];
            _phoneNumberId = configuration["WhatsApp:PhoneNumberId"];
        }

        public async Task SendVerificationCodeAsync(string telefone, string mensagem)
        {
            var token = _token;
            var phoneNumberId = _phoneNumberId;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = telefone,
                type = "template",
                template = new
                {
                    name = "verify_code",
                    language = new
                    {
                        code = "pt_BR"
                    },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = mensagem
                                }
                            }
                        },
                        new
                        {
                            type = "button",
                            sub_type = "url",
                            index = "0",
                            parameters = new[]
                            {
                                new
                                {
                                    type = "text",
                                    text = mensagem
                                }
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"https://graph.facebook.com/v22.0/{phoneNumberId}/messages", content);
            var respostaTexto = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Resposta: {respostaTexto}");
        }
    }
}
