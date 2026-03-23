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
    public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UsersControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Updating_User_Without_Token()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            object request = new
            {
                fullName = "Updated Integration User",
                birthDate = "1990-01-01"
            };

            // Act
            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/users/{userId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Update_User_When_Token_Is_Valid()
        {
            // Arrange
            AuthenticatedUser authenticatedUser = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticatedUser.AccessToken);

            object request = new
            {
                fullName = "Updated Integration User",
                birthDate = "1992-04-15"
            };

            // Act
            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/users/{authenticatedUser.UserId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("code").GetString().Should().Be(authenticatedUser.UserId.ToString());
        }

        [Theory]
        [InlineData(null, "O campo nome é obrigatório")]
        [InlineData("", "O campo nome é obrigatório")]
        [InlineData("   ", "O campo nome é obrigatório")]
        [InlineData("Abcd", "O campo nome deve conter 5 ou mais caracteres")]
        public async Task Should_Return_BadRequest_When_Name_Is_Invalid(string? fullName, string expectedMessage)
        {
            // Arrange
            AuthenticatedUser authenticatedUser = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticatedUser.AccessToken);

            object request = new
            {
                fullName,
                birthDate = "1992-04-15"
            };

            // Act
            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/users/{authenticatedUser.UserId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be(expectedMessage);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Name_Has_More_Than_250_Characters()
        {
            // Arrange
            AuthenticatedUser authenticatedUser = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticatedUser.AccessToken);

            object request = new
            {
                fullName = new string('a', 251),
                birthDate = "1992-04-15"
            };

            // Act
            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/users/{authenticatedUser.UserId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("O campo nome deve conter no máximo 250 caracteres.");
        }

        [Theory]
        [InlineData("3000-01-01")]
        [InlineData("2023-02-29")]
        [InlineData("31/01/2020")]
        [InlineData("2024-13-10")]
        public async Task Should_Return_BadRequest_When_BirthDate_Is_Invalid(string birthDate)
        {
            // Arrange
            AuthenticatedUser authenticatedUser = await CreateAndAuthenticateUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticatedUser.AccessToken);

            object request = new
            {
                fullName = "Updated Integration User",
                birthDate
            };

            // Act
            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/users/{authenticatedUser.UserId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Data de nascimento inválida.");
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

            accessToken.Should().NotBeNullOrWhiteSpace();

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
