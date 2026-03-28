using CafeSystem.Domain.Entities;
using CafeSystem.Infra.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CafeSystem.API.IntegrationTests
{
    public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public CategoriesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Creating_Category_Without_Token()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/categories", new { description = "Bebidas" });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Abcd")]
        [InlineData("Categoria#1")]
        public async Task Should_Return_BadRequest_When_Description_Is_Invalid(string description)
        {
            AuthenticatedUser authenticatedUser = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticatedUser.AccessToken);

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/categories", new { description });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números");
        }

        [Fact]
        public async Task Should_Return_Created_When_Category_Is_Valid()
        {
            AuthenticatedUser authenticatedUser = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticatedUser.AccessToken);

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/categories", new { description = "Bebidas" });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("code").GetInt32().Should().BeGreaterThan(0);
        }

        private async Task<AuthenticatedUser> CreateAndAuthenticateUserAsync(DateTime? expiresAtUtc = null)
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            string email = $"{Guid.NewGuid()}@example.com";

            object createRequest = new
            {
                fullName = "Integration User",
                email,
                password = "secret1",
                birthDate = "1990-01-01"
            };

            HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/users", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            JsonElement createBody = await ReadJsonBody(createResponse);
            Guid userId = Guid.Parse(createBody.GetProperty("code").GetString()!);

            string accessToken = Guid.NewGuid().ToString("N");
            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            RefreshToken refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = accessToken,
                UserId = userId,
                ExpiresAt = expiresAtUtc ?? DateTime.UtcNow.AddHours(1)
            };

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return new AuthenticatedUser
            {
                AccessToken = accessToken
            };
        }

        private static async Task<JsonElement> ReadJsonBody(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            return jsonDocument.RootElement.Clone();
        }

        private class AuthenticatedUser
        {
            public string AccessToken { get; set; } = string.Empty;
        }
    }
}
