using Microsoft.Extensions.DependencyInjection;
using WorshipApplication.Services;

namespace WorshipApplication
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Services
            services.AddScoped<AuthService>();
            services.AddScoped<HomeService>();
            services.AddScoped<MusicService>();
            services.AddScoped<ScheduleService>();
            services.AddScoped<UserService>();
            services.AddScoped<FcmNotificationService>();

            return services;
        }
    }
}
