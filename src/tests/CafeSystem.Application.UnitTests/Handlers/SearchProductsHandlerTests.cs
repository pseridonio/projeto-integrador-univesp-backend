using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class SearchProductsHandlerTests
    {
        [Fact]
        public async Task Should_Return_Products_When_Search_Is_Valid()
        {
            List<Product> products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Barcode = "7891234567890",
                    Description = "Suco de Laranja",
                    UnitPrice = 12.34m,
                    ProductCategories = new List<ProductCategory>
                    {
                        new ProductCategory
                        {
                            CategoryCode = 2,
                            Category = new Category { Code = 2, Description = "Bebidas" }
                        }
                    }
                }
            };

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.SearchAsync(null, "suco", 2, null, true, "description", It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            SearchProductsHandler handler = new SearchProductsHandler(productRepositoryMock.Object);
            SearchProductsRequest request = new SearchProductsRequest
            {
                Description = "suco",
                CategoryId = 2,
                IncludeCategories = true,
                Sort = "description"
            };

            List<SearchProductsResponse> response = await handler.HandleAsync(request);

            response.Should().HaveCount(1);
            response[0].Id.Should().Be(1);
            response[0].Categories.Should().NotBeNull();
            response[0].Categories.Should().HaveCount(1);
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_No_Products_Are_Found()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.SearchAsync(null, null, null, null, false, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            SearchProductsHandler handler = new SearchProductsHandler(productRepositoryMock.Object);
            SearchProductsRequest request = new SearchProductsRequest();

            Func<Task> act = async () => await handler.HandleAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }
    }
}
