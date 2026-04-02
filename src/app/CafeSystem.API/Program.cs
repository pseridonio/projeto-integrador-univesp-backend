using CafeSystem.Application.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
// Note: DI extension methods are invoked with fully-qualified names to avoid ambiguity between layers

namespace CafeSystem.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

            // JSON options: keep defaults for now

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Authentication and Authorization
            // NOTE (production): The configuration below is intentionally permissive for local/dev use.
            // - In production you should validate the issuer, audience and the signing key.
            // - Prefer asymmetric signing (RS256) with the private key kept secret and the public key exposed to the validators.
            // - Store signing keys and secrets in a secret store (Azure Key Vault, environment variables, etc.).
            // - Enforce shorter token lifetimes and ValidateLifetime = true.
            // - Consider rotating keys and maintaining a revocation list for compromised tokens.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Basic dev configuration; production should use secure storage for secrets
                // Keep the following commented suggestions in mind for production environments:
                // options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                // {
                //     ValidateIssuer = true,
                //     ValidIssuer = builder.Configuration["Jwt:Issuer"],
                //     ValidateAudience = true,
                //     ValidAudience = builder.Configuration["Jwt:Audience"],
                //     ValidateIssuerSigningKey = true,
                //     IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                //         System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                //     ValidateLifetime = true,
                //     ClockSkew = TimeSpan.FromSeconds(30)
                // };
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = false
                };
            });
            builder.Services.AddAuthorization();

            // Register API filters and controllers
            builder.Services.AddScoped<CafeSystem.API.Filters.AuthorizationFilter>();
            builder.Services
                .AddControllers(options =>
            {
                // Add the custom authorization filter globally; it will skip endpoints with [AllowAnonymous]
                options.Filters.AddService<CafeSystem.API.Filters.AuthorizationFilter>();
            })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        IEnumerable<string> allErrors = context.ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage)
                            .Where(x => !string.IsNullOrWhiteSpace(x));

                        string message = allErrors.FirstOrDefault() ?? "Dados inválidos";
                        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { message });
                    };
                });

            builder.Services.AddFluentValidationAutoValidation(config =>
            {
                config.DisableDataAnnotationsValidation = true;
            });
            builder.Services.AddFluentValidationClientsideAdapters();

            builder.Services.AddValidatorsFromAssemblyContaining<CafeSystem.API.Validators.LoginRequestValidator>();

            // Configure DI for each layer (fully-qualified to avoid ambiguous extension methods)
            CafeSystem.Application.Extensions.ServiceCollectionExtensions.ConfigureDI(builder.Services);
            CafeSystem.Infra.Extensions.ServiceCollectionExtensions.ConfigureDI(builder.Services);
            CafeSystem.API.Extensions.ServiceCollectionExtensions.ConfigureDI(builder.Services);
            if (builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddDbContext<CafeSystem.Infra.Persistence.AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("cafesystem-testing-db");
                });
            }
            else
            {
                // DbContext: configure PostgreSQL using connection string from configuration
                string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                builder.Services.AddDbContext<CafeSystem.Infra.Persistence.AppDbContext>(options =>
                {
                    options.UseNpgsql(connectionString, npgsql =>
                    {
                        npgsql.EnableRetryOnFailure();
                    });
                });
            }

            // Health checks: verifies DB connectivity
            builder.Services.AddHealthChecks()
                .AddCheck<DbHealthCheck>("db");

            Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // Use authentication/authorization middlewares
            app.UseAuthentication();
            app.UseAuthorization();

            // Map attribute routed controllers (so our filter runs)
            app.MapControllers();

            // Health endpoint: returns 204 No Content when healthy, 500 when unhealthy
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "text/plain";
                    context.Response.StatusCode = report.Status == HealthStatus.Healthy
                        ? StatusCodes.Status204NoContent
                        : StatusCodes.Status500InternalServerError;

                    if (report.Status != HealthStatus.Healthy)
                    {
                        // minimal body on error for observability
                        await context.Response.WriteAsync("Unhealthy");
                    }
                }
            });

            // Apply pending EF Core migrations at startup only if requested via startup argument
            // Usage: dotnet run --project app/CafeSystem.API -- --migrate
            bool applyMigrations = args != null && args.Any(a => a.Equals("--migrate", System.StringComparison.OrdinalIgnoreCase));
            if (applyMigrations)
            {
                using (IServiceScope scope = app.Services.CreateScope())
                {
                    Microsoft.Extensions.Logging.ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
                    CafeSystem.Infra.Persistence.AppDbContext db = scope.ServiceProvider.GetRequiredService<CafeSystem.Infra.Persistence.AppDbContext>();
                    IDatabaseInitializer databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

                    logger.LogInformation("Applying EF Core migrations at startup...");
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied. Executando rotina de seed...");
                    await databaseInitializer.EnsureSeedDataAsync();
                }
            }

            await app.RunAsync();
        }

        // Simple health check implementation that attempts to connect to the EF Core DbContext
        private class DbHealthCheck : IHealthCheck
        {
            private readonly IServiceScopeFactory _scopeFactory;

            public DbHealthCheck(IServiceScopeFactory scopeFactory)
            {
                _scopeFactory = scopeFactory;
            }

            public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            {
                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    CafeSystem.Infra.Persistence.AppDbContext db = scope.ServiceProvider.GetRequiredService<CafeSystem.Infra.Persistence.AppDbContext>();
                    bool canConnect = await db.Database.CanConnectAsync(cancellationToken);
                    return canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Cannot connect to database");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy(ex.Message);
                }
            }
        }
    }
}
