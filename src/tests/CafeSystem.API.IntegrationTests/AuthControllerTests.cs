using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CafeSystem.API.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<CafeSystem.API.Program>>
    {
        private readonly WebApplicationFactory<CafeSystem.API.Program> _factory;
        private readonly System.Net.Http.HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<CafeSystem.API.Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_Should_Return_Created()
        {
            var request = new { Email = "integtest@example.com", Password = "pwd123" };

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}
