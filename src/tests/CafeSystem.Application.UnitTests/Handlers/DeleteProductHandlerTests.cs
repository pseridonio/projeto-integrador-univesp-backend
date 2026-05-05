using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class DeleteProductHandlerTests
    {
        [Fact]
        public async Task Should_Delete_Product_When_Product_Is_Active()
        {
            int id = 1;
            Product product = BuildProduct(id, "123456789", false, null);

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.GetActiveByIdNoTrackingAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            DeleteProductHandler handler = new DeleteProductHandler(productRepositoryMock.Object);

            await handler.HandleAsync(id);

            product.IsDeleted.Should().BeTrue();
            product.Barcode.Should().BeEmpty();
            product.DeletedAt.Should().NotBeNull();
            product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_When_Product_Does_Not_Exist()
        {
            int id = 1;

            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock
                .Setup(x => x.GetActiveByIdNoTrackingAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            DeleteProductHandler handler = new DeleteProductHandler(productRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(id);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
            productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static Product BuildProduct(int id, string barcode, bool isDeleted, DateTime? deletedAt)
        {
            return new Product
            {
                Id = id,
                Barcode = barcode,
                Description = "Café",
                UnitPrice = 10m,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = deletedAt
            };
        }
    }
}
