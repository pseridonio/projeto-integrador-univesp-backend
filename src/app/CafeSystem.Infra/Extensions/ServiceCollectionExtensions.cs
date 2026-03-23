using Microsoft.Extensions.DependencyInjection;
using CafeSystem.Application.Interfaces;
using CafeSystem.Infra.Persistence;
using CafeSystem.Infra.Security;

namespace CafeSystem.Infra.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDI(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
            services.AddScoped<CafeSystem.Application.Interfaces.IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<CafeSystem.Application.Interfaces.IUnitOfWork, CafeSystem.Infra.Persistence.UnitOfWork>();
            // Expose concrete implementation methods used by application (RevokeAllForUserAsync)

            return services;
        }
    }
}
