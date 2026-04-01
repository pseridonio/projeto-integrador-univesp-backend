using CafeSystem.Domain.Entities;
using CafeSystem.Infra.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/categories", new { description });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números");
        }

        [Fact]
        public async Task Should_Return_Created_When_Category_Is_Valid()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/categories", new { description = "Bebidas" });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("code").GetInt32().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Getting_Category_Without_Token()
        {
            HttpResponseMessage response = await _client.GetAsync("/api/categories/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.GetAsync($"/api/categories/{int.MaxValue}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Categoria não encontrada.");
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Category_Is_Inactive()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: false, deletedAt: null);

            HttpResponseMessage response = await _client.GetAsync($"/api/categories/{code}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Category_Is_Deleted()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: false, deletedAt: DateTime.UtcNow);

            HttpResponseMessage response = await _client.GetAsync($"/api/categories/{code}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Return_Category_Data_When_Code_Is_Valid()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: true, deletedAt: null);

            HttpResponseMessage response = await _client.GetAsync($"/api/categories/{code}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("description").GetString().Should().Be("Bebidas");
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Updating_Category_Without_Token()
        {
            HttpResponseMessage response = await _client.PutAsJsonAsync("/api/categories/1", new { description = "Cafés" });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Abcd")]
        [InlineData("Categoria#1")]
        public async Task Should_Return_BadRequest_When_Update_Description_Is_Invalid(string description)
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: true, deletedAt: null);

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/categories/{code}", new { description });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números");
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Updating_Category_Does_Not_Exist()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/categories/{int.MaxValue}", new { description = "Cafés" });

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Categoria não encontrada.");
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Category_Is_Updated()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: true, deletedAt: null);

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/categories/{code}", new { description = "Cafés" });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Category? category = await dbContext.Categories.FindAsync(code);

            category.Should().NotBeNull();
            category!.Description.Should().Be("Cafés");
            category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public async Task Should_Return_NoContent_When_Updating_Category_With_Boundary_Length_Description(string description)
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: true, deletedAt: null);

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/categories/{code}", new { description });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Updating_Category_With_Same_Description()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: true, deletedAt: null);

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/categories/{code}", new { description = "Bebidas" });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Deleting_Category_Without_Token()
        {
            HttpResponseMessage response = await _client.DeleteAsync("/api/categories/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Deleting_Category_Does_Not_Exist()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.DeleteAsync($"/api/categories/{int.MaxValue}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Categoria não encontrada.");
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Deleting_Already_Deleted_Category()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: false, deletedAt: DateTime.UtcNow);

            HttpResponseMessage response = await _client.DeleteAsync($"/api/categories/{code}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Deleting_Active_Category_And_Mark_It_As_Deleted()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: true, deletedAt: null);

            HttpResponseMessage response = await _client.DeleteAsync($"/api/categories/{code}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Category? category = await dbContext.Categories.FindAsync(code);

            category.Should().NotBeNull();
            category!.IsActive.Should().BeFalse();
            category.DeletedAt.Should().NotBeNull();
            category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Deleting_Category_With_Maximum_Supported_Code()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int code = await CreateCategoryInDatabaseAsync(isActive: true, deletedAt: null, code: int.MaxValue);

            HttpResponseMessage response = await _client.DeleteAsync($"/api/categories/{code}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Authentication helper: prefer using IntegrationTestHelpers.AuthenticateAsAdminAsync

        private static async Task<JsonElement> ReadJsonBody(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            return jsonDocument.RootElement.Clone();
        }

        private async Task<int> CreateCategoryInDatabaseAsync(bool isActive, DateTime? deletedAt, int? code = null)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Category category = new Category
            {
                Description = "Bebidas",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = isActive,
                DeletedAt = deletedAt
            };

            if (code.HasValue)
            {
                category.Code = code.Value;
            }

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return category.Code;
        }

        private class AuthenticatedUser
        {
            public string AccessToken { get; set; } = string.Empty;
        }
    }
}
