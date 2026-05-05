using System.Net.Http;

namespace CafeSystem.API.IntegrationTests
{
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        protected CustomWebApplicationFactory _factory { get; }

        protected HttpClient _client { get; }

        public async Task InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}