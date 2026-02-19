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
        private readonly string _verificationCode;
        private readonly string _collectingAvailability;

        public WhatsAppService(IConfiguration configuration)
        {
            _token = configuration["WhatsApp:ApiToken"];
            _phoneNumberId = configuration["WhatsApp:PhoneNumberId"];
            _verificationCode = configuration["WhatsApp:Templates:VerificationCode"];
            _collectingAvailability = configuration["WhatsApp:Templates:CollectingAvailability"];
        }

        private async Task SendsAsync(object payload)
        {
            var token = _token;
            var phoneNumberId = _phoneNumberId;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await client.PostAsync($"https://graph.facebook.com/v22.0/{phoneNumberId}/messages", content);
        }

        public async Task SendVerificationCodeAsync(string phone, string code)
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = phone,
                type = "template",
                template = new
                {
                    name = _verificationCode,
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
                                    text = code
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
                                    text = code
                                }
                            }
                        }
                    }
                }
            };

            await SendsAsync(payload);
        }

        public async Task SendsScheduleNotificationAsync(string phone, string name)
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                to = phone,
                type = "template",
                template = new
                {
                    name = _collectingAvailability,
                    language = new
                    {
                        code = "pt_BR"
                    },
                    //components = new object[]
                    //{
                    //    new
                    //    {
                    //        type = "body",
                    //        parameters = new[]
                    //        {
                    //            new
                    //            {
                    //                type = "text",
                    //                text = name
                    //            }
                    //        }
                    //    }
                    //}
                }
            };

            await SendsAsync(payload);
        }
    }
}
