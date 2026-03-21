using Microsoft.Extensions.DependencyInjection;

namespace CafeSystem.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDI(this IServiceCollection services)
        {
            // API-level services (if any) can be registered here in future
            return services;
        }
    }
}
