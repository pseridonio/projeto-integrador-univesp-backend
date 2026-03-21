using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CafeSystem.API.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly System.Net.Http.HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Create_User_When_Request_Is_Valid()
        {
            // Arrange
            object request = new
            {
                fullName = "Integration User",
                email = "integration.user@example.com",
                password = "secret1",
                birthDate = "1990-01-01"
            };

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            JsonElement body = await ReadJsonBody(response);
            body.TryGetProperty("code", out JsonElement codeProperty).Should().BeTrue();
            codeProperty.GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Email_Already_Exists()
        {
            // Arrange
            object request = new
            {
                fullName = "Duplicated User",
                email = "duplicated.user@example.com",
                password = "secret1",
                birthDate = "1990-01-01"
            };

            await _client.PostAsJsonAsync("/api/users", request);

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("E-mail já cadastro");
        }

        [Theory]
        [InlineData(null, "Campo nome é obrigatório")]
        [InlineData("", "Campo nome é obrigatório")]
        [InlineData("   ", "Campo nome é obrigatório")]
        [InlineData("Abcd", "Nome deve conter 5 ou mais caracteres")]
        public async Task Should_Return_BadRequest_When_FullName_Is_Invalid(string? fullName, string expectedMessage)
        {
            object request = new
            {
                fullName,
                email = $"{System.Guid.NewGuid()}@example.com",
                password = "secret1",
                birthDate = "1990-01-01"
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be(expectedMessage);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_FullName_Has_More_Than_250_Characters()
        {
            object request = new
            {
                fullName = new string('a', 251),
                email = $"{System.Guid.NewGuid()}@example.com",
                password = "secret1",
                birthDate = "1990-01-01"
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Nome deve conter no máximo 250 caracteres");
        }

        [Theory]
        [InlineData(null, "Campo e-mail é obrigatório")]
        [InlineData("", "Campo e-mail é obrigatório")]
        [InlineData("   ", "Campo e-mail é obrigatório")]
        [InlineData("invalido", "Campo e-mail está em um formato inválido")]
        [InlineData("teste@", "Campo e-mail está em um formato inválido")]
        public async Task Should_Return_BadRequest_When_Email_Is_Invalid(string? email, string expectedMessage)
        {
            object request = new
            {
                fullName = "Integration User",
                email,
                password = "secret1",
                birthDate = "1990-01-01"
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData(null, "O campo senha é obrigatório")]
        [InlineData("", "O campo senha é obrigatório")]
        [InlineData("   ", "O campo senha é obrigatório")]
        [InlineData("1234", "Senha deve conter 5 ou mais caracteres")]
        [InlineData("123456789012345678901", "Senha deve conter no máximo 20 caracteres")]
        public async Task Should_Return_BadRequest_When_Password_Is_Invalid(string? password, string expectedMessage)
        {
            object request = new
            {
                fullName = "Integration User",
                email = $"{System.Guid.NewGuid()}@example.com",
                password,
                birthDate = "1990-01-01"
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be(expectedMessage);
        }

        [Theory]
        [InlineData("3000-01-01")]
        [InlineData("2023-02-29")]
        [InlineData("2024-13-10")]
        [InlineData("31/01/2020")]
        public async Task Should_Return_BadRequest_When_BirthDate_Is_Invalid(string birthDate)
        {
            object request = new
            {
                fullName = "Integration User",
                email = $"{System.Guid.NewGuid()}@example.com",
                password = "secret1",
                birthDate
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/users", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Data de nascimento inválida");
        }

        private static async Task<JsonElement> ReadJsonBody(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            return jsonDocument.RootElement.Clone();
        }
    }
}
