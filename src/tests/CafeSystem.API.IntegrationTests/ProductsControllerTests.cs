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
    public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ProductsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Creating_Product_Without_Token()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", new
            {
                barcode = "7891234567890",
                description = "Suco de Laranja",
                unitPrice = 12.34m,
                categories = new[] { 1 }
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Payload_Is_Invalid()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", new
            {
                barcode = "7891234567890",
                description = "Su",
                unitPrice = -1m,
                categories = Array.Empty<int>()
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Barcode_Is_Duplicated()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int categoryCode = await CreateCategoryInDatabaseAsync();
            await CreateProductInDatabaseAsync("7891234567890");

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", new
            {
                barcode = "7891234567890",
                description = "Suco de Laranja",
                unitPrice = 12.34m,
                categories = new[] { categoryCode }
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Código de barras já utilizado");
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Category_Is_Invalid()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", new
            {
                barcode = "7891234567000",
                description = "Suco de Laranja",
                unitPrice = 12.34m,
                categories = new[] { 999999 }
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Categoria inválida");
        }

        [Fact]
        public async Task Should_Return_Created_When_Request_Is_Valid()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int firstCategoryCode = await CreateCategoryInDatabaseAsync("Bebidas");
            int secondCategoryCode = await CreateCategoryInDatabaseAsync("Doces");

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", new
            {
                barcode = "7891234567111",
                description = "Suco de Laranja",
                unitPrice = 12.34m,
                categories = new[] { firstCategoryCode, secondCategoryCode }
            });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            int productId = body.GetProperty("id").GetInt32();
            productId.Should().BeGreaterThan(0);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Product? product = await dbContext.Products
                .Include(x => x.ProductCategories)
                .FirstOrDefaultAsync(x => x.Id == productId);

            product.Should().NotBeNull();
            product!.Barcode.Should().Be("7891234567111");
            product.ProductCategories.Should().HaveCount(2);
        }

        private async Task<int> CreateCategoryInDatabaseAsync(string description = "Bebidas")
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Category category = new Category
            {
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                DeletedAt = null
            };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return category.Code;
        }

        private async Task CreateProductInDatabaseAsync(string barcode)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Product product = new Product
            {
                Barcode = barcode,
                Description = "Produto Existente",
                UnitPrice = 5.50m,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
        }
    }
}
