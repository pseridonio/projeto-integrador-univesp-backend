using CafeSystem.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<ChangePasswordHandler>();
            services.AddScoped<UpdateUserHandler>();
            services.AddScoped<DeleteUserHandler>();
            services.AddScoped<GetUserByIdHandler>();
            services.AddScoped<GetUsersHandler>();
            services.AddScoped<CreateCategoryHandler>();
            services.AddScoped<GetCategoryByCodeHandler>();
            services.AddScoped<UpdateCategoryHandler>();
            services.AddScoped<DeleteCategoryHandler>();
            services.AddScoped<SearchCategoriesHandler>();

            // Repositories from infra are registered in infra layer; keep application only aware of handlers

            return services;
        }
    }
}
