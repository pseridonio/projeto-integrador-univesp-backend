using System;
using CafeSystem.Infra.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Xunit;

namespace CafeSystem.API.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<CafeSystem.API.Program>, IAsyncLifetime
    {
        private PostgreSqlContainer _postgreSqlContainer = null!;
        private string _connectionString = string.Empty;

        public async Task InitializeAsync()
        {
            // Ensure the test host will use the Development environment so the application
            // registers the PostgreSQL provider with the connection string we inject.
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            ConfigureDockerEndpointForWsl();

            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17")
                .WithDatabase("cafesystem_integration_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _postgreSqlContainer.StartAsync();

            _connectionString = _postgreSqlContainer.GetConnectionString();

            DbContextOptions<AppDbContext> dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using AppDbContext dbContext = new AppDbContext(dbContextOptions);
            await dbContext.Database.MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
            await base.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Use Development so Program.cs will configure Npgsql using the connection string we set below.
            builder.UseEnvironment("Development");

            // Override the DefaultConnection before the application configures DbContext.
            builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        }

        private static void ConfigureDockerEndpointForWsl()
        {
            string? dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
            if (string.IsNullOrWhiteSpace(dockerHost))
            {
                Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://localhost:2375");
            }

            string? hostOverride = Environment.GetEnvironmentVariable("TESTCONTAINERS_HOST_OVERRIDE");
            if (string.IsNullOrWhiteSpace(hostOverride))
            {
                Environment.SetEnvironmentVariable("TESTCONTAINERS_HOST_OVERRIDE", "localhost");
            }
        }
    }
}
