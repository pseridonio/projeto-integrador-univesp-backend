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
    public class UsersListTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UsersListTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Token_Is_Missing()
        {
            HttpResponseMessage response = await _client.GetAsync("/api/users");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_All_Active_Users_When_No_Filters_Are_Provided()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            Guid activeOneId = await SeedUserAsync("Maria Jose Inacio", $"active-{Guid.NewGuid():N}@contoso.com");
            Guid activeTwoId = await SeedUserAsync("Jose Pereira", $"active-{Guid.NewGuid():N}@contoso.com");
            Guid deletedUserId = await CreateDeletedUserAsync("Maria do Carmo", $"deleted-{Guid.NewGuid():N}@contoso.com");

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonElement body = await ReadJsonBody(response);
            body.ValueKind.Should().Be(JsonValueKind.Array);
            List<JsonElement> users = body.EnumerateArray().ToList();
            users.Should().Contain(item => item.GetProperty("code").GetGuid() == auth.UserId);
            users.Should().Contain(item => item.GetProperty("code").GetGuid() == activeOneId);
            users.Should().Contain(item => item.GetProperty("code").GetGuid() == activeTwoId);
            users.Should().NotContain(item => item.GetProperty("code").GetGuid() == deletedUserId);
        }

        [Fact]
        public async Task Should_Return_Only_User_That_Matches_All_Name_And_Email_Terms()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            string emailToken = Guid.NewGuid().ToString("N");
            Guid matchingUserId = await SeedUserAsync("Maria Jose Inacio", $"{emailToken}@contoso.com");
            await SeedUserAsync("Jose Maria Inacio", $"other-{Guid.NewGuid():N}@contoso.com");
            await SeedUserAsync("Maria Jose Inacio", $"other-{Guid.NewGuid():N}@other.com");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/users?name=Maria&name=Jose&email={emailToken}&email=contoso");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonElement body = await ReadJsonBody(response);
            body.ValueKind.Should().Be(JsonValueKind.Array);
            List<JsonElement> users = body.EnumerateArray().ToList();
            users.Should().HaveCount(1);
            users[0].GetProperty("code").GetGuid().Should().Be(matchingUserId);
            users[0].GetProperty("fullName").GetString().Should().Be("Maria Jose Inacio");
            users[0].GetProperty("email").GetString().Should().Be($"{emailToken}@contoso.com");
        }

        [Fact]
        public async Task Should_Return_NotFound_When_No_User_Matches_Filters()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            await SeedUserAsync("Maria Jose Inacio", $"{Guid.NewGuid():N}@contoso.com");

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/users?name=Nonexistent&email=missing");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Interpret_Multiple_Query_Terms_As_Conjunctive_Filters()
        {
            // Arrange
            AuthenticatedUser auth = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

            string emailToken = Guid.NewGuid().ToString("N");
            Guid matchingUserId = await SeedUserAsync("Maria Jose Inacio", $"{emailToken}@contoso.com");
            await SeedUserAsync("Maria Jose Inacio", $"other-{Guid.NewGuid():N}@other.com");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/users?name=Maria,Jose&email={emailToken},contoso");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonElement body = await ReadJsonBody(response);
            List<JsonElement> users = body.EnumerateArray().ToList();
            users.Should().HaveCount(1);
            users[0].GetProperty("code").GetGuid().Should().Be(matchingUserId);
        }

        private async Task<AuthenticatedUser> CreateAndAuthenticateUserAsync()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            Guid userId = await SeedUserAsync("Integration User", $"{Guid.NewGuid()}@example.com");

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

        private async Task<Guid> SeedUserAsync(string fullName, string email, bool isActive = true)
        {
            Guid userId = Guid.NewGuid();

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            User user = new User
            {
                Id = userId,
                FullName = fullName,
                Email = email,
                PasswordHash = "hashed-password",
                PasswordSalt = "salt",
                BirthDate = new DateOnly(1990, 1, 1),
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return userId;
        }

        private async Task<Guid> CreateDeletedUserAsync(string fullName, string email)
        {
            Guid userId = await SeedUserAsync(fullName, email, isActive: false);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            User? user = await dbContext.Users.FindAsync(userId);
            user!.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return userId;
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
