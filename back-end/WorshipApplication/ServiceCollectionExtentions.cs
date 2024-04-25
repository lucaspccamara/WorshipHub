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
            services.AddScoped<UsuarioService>();

            return services;
        }
    }
}
