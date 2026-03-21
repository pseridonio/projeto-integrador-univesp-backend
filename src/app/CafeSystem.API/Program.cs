using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
// Note: DI extension methods are invoked with fully-qualified names to avoid ambiguity between layers

namespace CafeSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
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
                    options.UseNpgsql(connectionString, npgsql => {
                        npgsql.EnableRetryOnFailure();
                    });
                });
            }

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

            // Apply pending EF Core migrations at startup only if requested via startup argument
            // Usage: dotnet run --project app/CafeSystem.API -- --migrate
            bool applyMigrations = args != null && args.Any(a => a.Equals("--migrate", System.StringComparison.OrdinalIgnoreCase));
            if (applyMigrations)
            {
                using (var scope = app.Services.CreateScope())
                {
                    Microsoft.Extensions.Logging.ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
                    CafeSystem.Infra.Persistence.AppDbContext db = scope.ServiceProvider.GetRequiredService<CafeSystem.Infra.Persistence.AppDbContext>();

                    logger.LogInformation("Applying EF Core migrations at startup...");
                    db.Database.Migrate();
                    logger.LogInformation("Migrations applied.");
                }
            }

            
            app.Run();
        }
    }
}
