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
    public class ProductsControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
    {
        public ProductsControllerTests(CustomWebApplicationFactory factory)
            : base(factory)
        {
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
            body.GetProperty("message").GetString().Should().Be("Código de barras já utilizado.");
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

        [Fact]
        public async Task Should_Return_Unauthorized_When_Updating_Product_Without_Token()
        {
            HttpResponseMessage response = await _client.PutAsJsonAsync("/api/products/1", new
            {
                barcode = "7891234567000",
                description = "Produto Atualizado",
                unitPrice = 9.99m
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Updating_Product_That_Does_Not_Exist()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.PutAsJsonAsync("/api/products/999999", new
            {
                barcode = "7891234567000",
                description = "Produto Atualizado",
                unitPrice = 9.99m
            });

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Updating_Deleted_Product()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateDeletedProductInDatabaseAsync("7891234500000");

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/products/{productId}", new
            {
                barcode = "7891234567000",
                description = "Produto Atualizado",
                unitPrice = 9.99m
            });

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Updating_Product_With_Duplicated_Barcode()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int sourceProductId = await CreateProductInDatabaseAsync("7891234511111");
            await CreateProductInDatabaseAsync("7891234522222");

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/products/{sourceProductId}", new
            {
                barcode = "7891234522222",
                description = "Produto Atualizado",
                unitPrice = 10.99m
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Código de barras já utilizado.");
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Updating_Product_Keeping_Same_Barcode()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234533333");

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/products/{productId}", new
            {
                barcode = "7891234533333",
                description = "Produto Atualizado",
                unitPrice = 18.50m
            });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Product? product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

            product.Should().NotBeNull();
            product!.Barcode.Should().Be("7891234533333");
            product.Description.Should().Be("Produto Atualizado");
            product.UnitPrice.Should().Be(18.50m);
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Updating_Product_Changing_Barcode()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234544444");

            HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/products/{productId}", new
            {
                barcode = "7891234555555",
                description = "Produto Alterado",
                unitPrice = 21.00m
            });

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Product? product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

            product.Should().NotBeNull();
            product!.Barcode.Should().Be("7891234555555");
            product.Description.Should().Be("Produto Alterado");
            product.UnitPrice.Should().Be(21.00m);
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Deleting_Product_Without_Token()
        {
            HttpResponseMessage response = await _client.DeleteAsync("/api/products/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Deleting_Product_That_Does_Not_Exist()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.DeleteAsync($"/api/products/{int.MaxValue}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Produto não encontrado.");
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Deleting_Active_Product_And_Clear_Barcode()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234567777");

            HttpResponseMessage response = await _client.DeleteAsync($"/api/products/{productId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Product? product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);

            product.Should().NotBeNull();
            product!.IsDeleted.Should().BeTrue();
            product.Barcode.Should().BeEmpty();
            product.DeletedAt.Should().NotBeNull();
            product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Adding_Category_To_Product_Without_Token()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products/1/categories/1", new { });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Adding_Category_To_Deleted_Product()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateDeletedProductInDatabaseAsync("7891234599999");
            int categoryCode = await CreateCategoryInDatabaseAsync("Bebidas");

            HttpResponseMessage response = await _client.PostAsync($"/api/products/{productId}/categories/{categoryCode}", null);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Produto não encontrado.");
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Category_Is_Invalid_On_Association()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234600000");

            HttpResponseMessage response = await _client.PostAsync($"/api/products/{productId}/categories/999999", null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Categoria inválida");
        }

        [Fact]
        public async Task Should_Return_Created_When_Adding_Category_To_Product()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234611111");
            int categoryCode = await CreateCategoryInDatabaseAsync("Lanches");

            HttpResponseMessage response = await _client.PostAsync($"/api/products/{productId}/categories/{categoryCode}", null);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            bool associationExists = await dbContext.ProductCategories.AnyAsync(x => x.ProductId == productId && x.CategoryCode == categoryCode);

            associationExists.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Association_Already_Exists()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234622222");
            int categoryCode = await CreateCategoryInDatabaseAsync("Padaria");

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.ProductCategories.Add(new ProductCategory
            {
                ProductId = productId,
                CategoryCode = categoryCode
            });
            await dbContext.SaveChangesAsync();

            HttpResponseMessage response = await _client.PostAsync($"/api/products/{productId}/categories/{categoryCode}", null);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Removing_Category_From_Product_Without_Token()
        {
            HttpResponseMessage response = await _client.DeleteAsync("/api/products/1/categories/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_Removing_Category_From_Deleted_Product()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateDeletedProductInDatabaseAsync("7891234633333");
            int categoryCode = await CreateCategoryInDatabaseAsync("Bebidas");

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.ProductCategories.Add(new ProductCategory
            {
                ProductId = productId,
                CategoryCode = categoryCode
            });
            await dbContext.SaveChangesAsync();

            HttpResponseMessage response = await _client.DeleteAsync($"/api/products/{productId}/categories/{categoryCode}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Produto não encontrado.");
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Category_Is_Not_Associated()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234644444");
            int categoryCode = await CreateCategoryInDatabaseAsync("Lanches");

            HttpResponseMessage response = await _client.DeleteAsync($"/api/products/{productId}/categories/{categoryCode}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Categoria inválida");
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Removing_Last_Category()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234655555");
            int categoryCode = await CreateCategoryInDatabaseAsync("Padaria");

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.ProductCategories.Add(new ProductCategory
            {
                ProductId = productId,
                CategoryCode = categoryCode
            });
            await dbContext.SaveChangesAsync();

            HttpResponseMessage response = await _client.DeleteAsync($"/api/products/{productId}/categories/{categoryCode}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("O produto deve ter ao menos 1 categoria");
        }

        [Fact]
        public async Task Should_Return_NoContent_When_Removing_Category_From_Product()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int productId = await CreateProductInDatabaseAsync("7891234666666");
            int categoryCode1 = await CreateCategoryInDatabaseAsync("Bebidas");
            int categoryCode2 = await CreateCategoryInDatabaseAsync("Doces");

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.ProductCategories.AddRange(
                new ProductCategory { ProductId = productId, CategoryCode = categoryCode1 },
                new ProductCategory { ProductId = productId, CategoryCode = categoryCode2 });
            await dbContext.SaveChangesAsync();

            HttpResponseMessage response = await _client.DeleteAsync($"/api/products/{productId}/categories/{categoryCode1}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using IServiceScope verifyScope = _factory.Services.CreateScope();
            AppDbContext verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            bool associationExists = await verifyDbContext.ProductCategories.AnyAsync(x => x.ProductId == productId && x.CategoryCode == categoryCode1);

            associationExists.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Return_Unauthorized_When_Searching_Products_Without_Token()
        {
            HttpResponseMessage response = await _client.GetAsync("/api/products");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Should_Return_BadRequest_When_Search_Query_Is_Invalid()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.GetAsync("/api/products?sort=invalid");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Return_NotFound_When_No_Products_Match_Search()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            HttpResponseMessage response = await _client.GetAsync("/api/products?description=Inexistente");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetProperty("message").GetString().Should().Be("Nenhum produto encontrado");
        }

        [Fact]
        public async Task Should_Return_Products_When_Filter_Is_Valid()
        {
            await IntegrationTestHelpers.AuthenticateAsAdminAsync(_client);

            int categoryCode = await CreateCategoryInDatabaseAsync("Bebidas");
            int productId = await CreateProductInDatabaseAsync("7891234677777");

            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.ProductCategories.Add(new ProductCategory
            {
                ProductId = productId,
                CategoryCode = categoryCode
            });
            await dbContext.SaveChangesAsync();

            HttpResponseMessage response = await _client.GetAsync($"/api/products?id={productId}&description=Produto&categoryId={categoryCode}&barcode=7891234677777&includeCategories=true&sort=-unitPrice");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            JsonElement body = await IntegrationTestHelpers.ReadJsonBodyAsync(response);
            body.GetArrayLength().Should().Be(1);
            body[0].GetProperty("id").GetInt32().Should().Be(productId);
            body[0].TryGetProperty("categories", out JsonElement categories).Should().BeTrue();
            categories.GetArrayLength().Should().Be(1);
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

        private async Task<int> CreateProductInDatabaseAsync(string barcode)
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

            return product.Id;
        }

        private async Task<int> CreateDeletedProductInDatabaseAsync(string barcode)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Product product = new Product
            {
                Barcode = barcode,
                Description = "Produto Excluído",
                UnitPrice = 5.50m,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return product.Id;
        }
    }
}
