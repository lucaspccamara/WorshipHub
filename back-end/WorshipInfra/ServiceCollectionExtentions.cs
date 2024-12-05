using Microsoft.Extensions.DependencyInjection;
using WorshipDomain.Repository;
using WorshipInfra.Database.Interfaces;
using WorshipInfra.Database;
using WorshipInfra.Repository;

namespace WorshipInfra
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Selecting the Dapper SimpleCRUD Dialect for MySQL
            Dapper.SimpleCRUD.SetDialect(Dapper.SimpleCRUD.Dialect.MySQL);

            // Context
            services.AddScoped<IContextRepository, ContextRepository>();

            // Repositories
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
