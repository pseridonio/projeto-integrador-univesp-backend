using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CafeSystem.Domain.Entities;
using CafeSystem.Infra.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CafeSystem.API.IntegrationTests
{
    public class UsersGetTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UsersGetTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Token_Is_Missing()
        {
            HttpResponseMessage response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Code_Is_Invalid()
        {
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            HttpResponseMessage response = await _client.GetAsync("/api/users/invalid-guid");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Código informado é inválido");
        }

        [Fact]
        public async Task Should_Return_NotFound_When_User_Does_Not_Exist()
        {
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            HttpResponseMessage response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_User_Is_Deleted()
        {
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                User? user = await db.Users.FindAsync(auth.UserId);
                user!.IsActive = false;
                user.DeletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            HttpResponseMessage response = await _client.GetAsync($"/api/users/{auth.UserId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Return_User_Data_When_Code_Is_Valid()
        {
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            HttpResponseMessage response = await _client.GetAsync($"/api/users/{auth.UserId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("code").GetGuid().Should().Be(auth.UserId);
            body.GetProperty("fullName").GetString().Should().Be("Integration User");
            body.GetProperty("email").GetString().Should().EndWith("@example.com");
            body.GetProperty("birthDate").GetString().Should().Be("1990-01-01");
        }

        private async Task<AuthenticatedUser> CreateAndAuthenticateUserAsync()
        {
            string email = $"{Guid.NewGuid()}@example.com";
            string password = "secret1";

            object createRequest = new
            {
                fullName = "Integration User",
                email,
                password,
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
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            dbContext.RefreshTokens.Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return new AuthenticatedUser
            {
                UserId = userId,
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
            public Guid UserId { get; set; }

            public string AccessToken { get; set; } = string.Empty;
        }
    }
}
