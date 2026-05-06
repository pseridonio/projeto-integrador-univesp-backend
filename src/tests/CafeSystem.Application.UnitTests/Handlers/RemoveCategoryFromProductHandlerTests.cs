using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class RemoveCategoryFromProductHandlerTests
    {
        [Fact]
        public async Task Should_Remove_Category_From_Product_When_Valid()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            productRepositoryMock.Setup(x => x.ExistsCategoryAssociationAsync(10, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            productRepositoryMock.Setup(x => x.CountCategoryAssociationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(2);

            RemoveCategoryFromProductHandler handler = new RemoveCategoryFromProductHandler(productRepositoryMock.Object);

            await handler.HandleAsync(10, 2);

            productRepositoryMock.Verify(x => x.RemoveCategoryAsync(It.Is<ProductCategory>(pc => pc.ProductId == 10 && pc.CategoryCode == 2), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_Product_Does_Not_Exist()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            RemoveCategoryFromProductHandler handler = new RemoveCategoryFromProductHandler(productRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(10, 2);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Throw_BadRequest_When_Category_Is_Not_Associated()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            productRepositoryMock.Setup(x => x.ExistsCategoryAssociationAsync(10, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            RemoveCategoryFromProductHandler handler = new RemoveCategoryFromProductHandler(productRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(10, 2);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Categoria inválida");
        }

        [Fact]
        public async Task Should_Throw_BadRequest_When_Product_Would_Have_No_Categories()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            productRepositoryMock.Setup(x => x.ExistsCategoryAssociationAsync(10, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            productRepositoryMock.Setup(x => x.CountCategoryAssociationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(1);

            RemoveCategoryFromProductHandler handler = new RemoveCategoryFromProductHandler(productRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(10, 2);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("O produto deve ter ao menos 1 categoria");
        }
    }
}
