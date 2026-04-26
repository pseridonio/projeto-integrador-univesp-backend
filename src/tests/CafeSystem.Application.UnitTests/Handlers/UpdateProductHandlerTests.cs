using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class UpdateProductHandlerTests
    {
        [Fact]
        public async Task Should_Update_Product_When_Request_Is_Valid_And_Keeping_Same_Barcode()
        {
            int productId = 10;
            Product product = BuildProduct(productId, "7891234567890", isDeleted: false);

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.GetActiveByIdNoTrackingAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            productRepositoryMock
                .Setup(x => x.ExistsActiveByBarcodeExceptIdAsync("7891234567890", productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            UpdateProductHandler handler = new UpdateProductHandler(productRepositoryMock.Object);
            UpdateProductRequest request = new UpdateProductRequest
            {
                Barcode = "7891234567890",
                Description = "Produto Atualizado",
                UnitPrice = 19.90m
            };

            Product updatedProduct = await handler.HandleAsync(productId, request);

            updatedProduct.Barcode.Should().Be("7891234567890");
            updatedProduct.Description.Should().Be("Produto Atualizado");
            updatedProduct.UnitPrice.Should().Be(19.90m);
            updatedProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Update_Product_When_Request_Is_Valid_And_Changing_Barcode()
        {
            int productId = 10;
            Product product = BuildProduct(productId, "7891234567890", isDeleted: false);

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.GetActiveByIdNoTrackingAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            productRepositoryMock
                .Setup(x => x.ExistsActiveByBarcodeExceptIdAsync("7891234567000", productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            UpdateProductHandler handler = new UpdateProductHandler(productRepositoryMock.Object);
            UpdateProductRequest request = new UpdateProductRequest
            {
                Barcode = "7891234567000",
                Description = "Produto Atualizado",
                UnitPrice = 25.00m
            };

            Product updatedProduct = await handler.HandleAsync(productId, request);

            updatedProduct.Barcode.Should().Be("7891234567000");
            updatedProduct.Description.Should().Be("Produto Atualizado");
            updatedProduct.UnitPrice.Should().Be(25.00m);
            productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_Product_Does_Not_Exist()
        {
            int productId = 999;

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.GetActiveByIdNoTrackingAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            UpdateProductHandler handler = new UpdateProductHandler(productRepositoryMock.Object);
            UpdateProductRequest request = new UpdateProductRequest
            {
                Barcode = "7891234567890",
                Description = "Produto",
                UnitPrice = 10m
            };

            Func<Task> act = async () => await handler.HandleAsync(productId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Throw_BadRequest_When_Barcode_Is_Used_By_Another_Product()
        {
            int productId = 10;
            Product product = BuildProduct(productId, "7891234567890", isDeleted: false);

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.GetActiveByIdNoTrackingAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            productRepositoryMock
                .Setup(x => x.ExistsActiveByBarcodeExceptIdAsync("7891234567000", productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            UpdateProductHandler handler = new UpdateProductHandler(productRepositoryMock.Object);
            UpdateProductRequest request = new UpdateProductRequest
            {
                Barcode = "7891234567000",
                Description = "Produto Atualizado",
                UnitPrice = 15m
            };

            Func<Task> act = async () => await handler.HandleAsync(productId, request);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Código de barras já utilizado.");
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_Product_Is_Deleted()
        {
            int productId = 10;

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.GetActiveByIdNoTrackingAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            UpdateProductHandler handler = new UpdateProductHandler(productRepositoryMock.Object);
            UpdateProductRequest request = new UpdateProductRequest
            {
                Barcode = "7891234567890",
                Description = "Produto Atualizado",
                UnitPrice = 15m
            };

            Func<Task> act = async () => await handler.HandleAsync(productId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        private static Product BuildProduct(int id, string barcode, bool isDeleted)
        {
            return new Product
            {
                Id = id,
                Barcode = barcode,
                Description = "Produto",
                UnitPrice = 10m,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
        }
    }
}
