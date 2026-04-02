using CafeSystem.Domain.Entities;
using CafeSystem.Infra.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CafeSystem.API.IntegrationTests
{
    public class CategoriesSearchTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public CategoriesSearchTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Searching_Without_Token()
        {
            // Arrange
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Bebidas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Description_Is_Empty()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Descrição é obrigatória para realizar a busca");
        }

        [Fact]
        public async Task Should_Return_NotFound_When_No_Categories_Match_Search_Term()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Bebidas", isActive: true, deletedAt: null);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Inexistente");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await ReadJsonBody(response);
            body.GetProperty("message").GetString().Should().Be("Nenhuma categoria encontrada com os critérios especificados.");
        }

        [Fact]
        public async Task Should_Return_OK_When_Categories_Match_Search_Term()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Bebidas Quentes", isActive: true, deletedAt: null);
            await CreateCategoryInDatabaseAsync("Bebidas Frias", isActive: true, deletedAt: null);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Bebidas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;
            root.GetArrayLength().Should().Be(2);
        }

        [Fact]
        public async Task Should_Return_Single_Category_When_Only_One_Matches()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Alimentos Frescos", isActive: true, deletedAt: null);
            await CreateCategoryInDatabaseAsync("Bebidas Quentes", isActive: true, deletedAt: null);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Alimentos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;
            root.GetArrayLength().Should().Be(1);
            root[0].GetProperty("description").GetString().Should().Be("Alimentos Frescos");
        }

        [Fact]
        public async Task Should_Support_Multiple_Search_Terms()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Bebidas Quentes", isActive: true, deletedAt: null);
            await CreateCategoryInDatabaseAsync("Alimentos Frescos", isActive: true, deletedAt: null);
            await CreateCategoryInDatabaseAsync("Doces Açucarados", isActive: true, deletedAt: null);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Bebidas Alimentos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;
            root.GetArrayLength().Should().Be(2);
        }

        [Fact]
        public async Task Should_Not_Return_Inactive_Categories()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Bebidas Ativas", isActive: true, deletedAt: null);
            await CreateCategoryInDatabaseAsync("Bebidas Inativas", isActive: false, deletedAt: null);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Bebidas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;
            root.GetArrayLength().Should().Be(1);
            root[0].GetProperty("description").GetString().Should().Be("Bebidas Ativas");
        }

        [Fact]
        public async Task Should_Not_Return_Deleted_Categories()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Bebidas Ativas", isActive: true, deletedAt: null);
            await CreateCategoryInDatabaseAsync("Bebidas Deletadas", isActive: true, deletedAt: DateTime.UtcNow);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Bebidas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;
            root.GetArrayLength().Should().Be(1);
            root[0].GetProperty("description").GetString().Should().Be("Bebidas Ativas");
        }

        [Fact]
        public async Task Should_Return_Correct_Code_And_Description_In_Response()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            int categoryCode = await CreateCategoryInDatabaseAsync("Bebidas Especiais", isActive: true, deletedAt: null);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Bebidas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;
            root[0].GetProperty("code").GetInt32().Should().Be(categoryCode);
            root[0].GetProperty("description").GetString().Should().Be("Bebidas Especiais");
        }

        [Theory]
        [InlineData("Bebidas Quentes")]
        [InlineData("Quentes")]
        [InlineData("Bebidas")]
        public async Task Should_Find_Categories_With_Partial_Match(string searchTerm)
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Bebidas Quentes", isActive: true, deletedAt: null);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/categories?description={Uri.EscapeDataString(searchTerm)}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            JsonElement root = jsonDocument.RootElement;
            root.GetArrayLength().Should().Be(1);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_All_Matching_Categories_Are_Deleted()
        {
            // Arrange
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);
            await CreateCategoryInDatabaseAsync("Bebidas Deletadas", isActive: true, deletedAt: DateTime.UtcNow);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/categories?description=Bebidas");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static async Task<JsonElement> ReadJsonBody(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            return jsonDocument.RootElement.Clone();
        }

        private async Task<int> CreateCategoryInDatabaseAsync(string description, bool isActive, DateTime? deletedAt)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Category category = new Category
            {
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = isActive,
                DeletedAt = deletedAt
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return category.Code;
        }
    }
}
