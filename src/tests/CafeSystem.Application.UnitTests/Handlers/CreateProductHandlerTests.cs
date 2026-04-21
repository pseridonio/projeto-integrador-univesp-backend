using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class CreateProductHandlerTests
    {
        [Fact]
        public async Task Should_Create_Product_When_Request_Is_Valid()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.ExistsActiveByBarcodeAsync("7891234567890", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.CountActiveByCodesAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            CreateProductHandler handler = new CreateProductHandler(productRepositoryMock.Object, categoryRepositoryMock.Object);
            CreateProductRequest request = new CreateProductRequest
            {
                Barcode = "7891234567890",
                Description = "Suco de Laranja",
                UnitPrice = 12.34m,
                Categories = new List<int> { 1, 2 }
            };

            Product product = await handler.HandleAsync(request);

            product.Barcode.Should().Be("7891234567890");
            product.Description.Should().Be("Suco de Laranja");
            product.UnitPrice.Should().Be(12.34m);
            product.ProductCategories.Should().HaveCount(2);
            productRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Barcode_Is_Already_In_Use()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.ExistsActiveByBarcodeAsync("7891234567890", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();

            CreateProductHandler handler = new CreateProductHandler(productRepositoryMock.Object, categoryRepositoryMock.Object);
            CreateProductRequest request = new CreateProductRequest
            {
                Barcode = "7891234567890",
                Description = "Suco de Laranja",
                UnitPrice = 12.34m,
                Categories = new List<int> { 1 }
            };

            Func<Task> act = async () => await handler.HandleAsync(request);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Código de barras já utilizado");
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Any_Category_Is_Invalid()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.ExistsActiveByBarcodeAsync("7891234567890", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.CountActiveByCodesAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            CreateProductHandler handler = new CreateProductHandler(productRepositoryMock.Object, categoryRepositoryMock.Object);
            CreateProductRequest request = new CreateProductRequest
            {
                Barcode = "7891234567890",
                Description = "Suco de Laranja",
                UnitPrice = 12.34m,
                Categories = new List<int> { 1, 2 }
            };

            Func<Task> act = async () => await handler.HandleAsync(request);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Categoria inválida");
        }
    }
}
