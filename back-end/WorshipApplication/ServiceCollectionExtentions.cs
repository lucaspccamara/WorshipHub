using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WorshipApplication.Services;
using WorshipApplication.Workers;
using WorshipDomain.Configurations;
using WorshipDomain.Interfaces;

namespace WorshipApplication
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurations
            services.Configure<BrevoSettings>(configuration.GetSection("BrevoSettings"));

            // Services
            services.AddScoped<AuthService>();
            services.AddScoped<HomeService>();
            services.AddScoped<MusicService>();
            services.AddScoped<ScheduleService>();
            services.AddScoped<UserService>();
            services.AddScoped<FcmNotificationService>();

            // Email Service with HttpClient
            services.AddHttpClient<IEmailService, BrevoEmailService>();

            // Background Workers
            services.AddHostedService<EventReminderWorker>();

            return services;
        }
    }
}
