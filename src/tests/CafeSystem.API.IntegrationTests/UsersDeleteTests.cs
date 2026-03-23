using CafeSystem.Domain.Entities;
using CafeSystem.Infra.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CafeSystem.API.IntegrationTests
{
    public class UsersDeleteTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UsersDeleteTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Delete_NonExisting_User_Returns_404()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_Self_Returns_400()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/users/{auth.UserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            body.GetProperty("message").GetString().Should().Be("Não é possível excluir a si mesmo");
        }

        [Fact]
        public async Task Delete_Already_Deleted_User_Returns_204()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // mark user as deleted
                Domain.Entities.User? user = await db.Users.FindAsync(auth.UserId);
                user.IsActive = false;
                user.DeletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/users/{auth.UserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_Existing_User_Returns_204_And_Tokens_Revoke()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            // create an extra refresh token for this user
            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                RefreshToken rt = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = Guid.NewGuid().ToString("N"),
                    UserId = auth.UserId,
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
                };
                db.RefreshTokens.Add(rt);
                await db.SaveChangesAsync();
            }

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

            // call delete on the created user
            response = await _client.DeleteAsync($"/api/users/{auth.UserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var tokens = await db.RefreshTokens.Where(t => t.UserId == auth.UserId).ToListAsync();
                tokens.Should().OnlyContain(t => t.RevokedAt != null);

                var user = await db.Users.FindAsync(auth.UserId);
                user.IsActive.Should().BeFalse();
                user.DeletedAt.Should().NotBeNull();
            }
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
            var createBody = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
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

            accessToken.Should().NotBeNullOrWhiteSpace();

            return new AuthenticatedUser
            {
                UserId = userId,
                AccessToken = accessToken
            };
        }

        private class AuthenticatedUser
        {
            public Guid UserId { get; set; }

            public string AccessToken { get; set; } = string.Empty;
        }
    }
}
