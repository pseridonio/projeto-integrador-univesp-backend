using Microsoft.Extensions.DependencyInjection;
using CafeSystem.Application.Handlers;

namespace CafeSystem.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDI(this IServiceCollection services)
        {
            // Register application layer handlers and services
            services.AddScoped<LoginHandler>();
            services.AddScoped<RefreshTokenHandler>();
            services.AddScoped<RegisterHandler>();
            services.AddScoped<UpdateUserHandler>();
            services.AddScoped<DeleteUserHandler>();
            services.AddScoped<GetUserByIdHandler>();

            // Repositories from infra are registered in infra layer; keep application only aware of handlers

            return services;
        }
    }
}
