﻿using Microsoft.Extensions.DependencyInjection;
using WorshipApplication.Services;
using WorshipDomain.Repository;
using WorshipInfra.Database.Interfaces;
using WorshipInfra.Database;
using WorshipInfra.Repository;

namespace WorshipApplication
{
    public static class ServiceCollectionExtentions
    {
        public static void Load(IServiceCollection services)
        {
            // Context
            services.AddScoped<IContextRepository, ContextRepository>();

            // Repositories
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            // Services
            services.AddScoped<AuthService>();
            services.AddScoped<UsuarioService>();
        }
    }
}