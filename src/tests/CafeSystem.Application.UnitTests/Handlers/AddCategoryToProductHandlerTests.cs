using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class AddCategoryToProductHandlerTests
    {
        [Fact]
        public async Task Should_Add_Category_To_Product_When_Product_And_Category_Are_Valid()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            productRepositoryMock.Setup(x => x.ExistsCategoryAssociationAsync(10, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock.Setup(x => x.ExistsActiveByCodeAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            AddCategoryToProductHandler handler = new AddCategoryToProductHandler(productRepositoryMock.Object, categoryRepositoryMock.Object);

            bool created = await handler.HandleAsync(10, 2);

            created.Should().BeTrue();
            productRepositoryMock.Verify(x => x.AddCategoryAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Return_False_When_Association_Already_Exists()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            productRepositoryMock.Setup(x => x.ExistsCategoryAssociationAsync(10, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock.Setup(x => x.ExistsActiveByCodeAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            AddCategoryToProductHandler handler = new AddCategoryToProductHandler(productRepositoryMock.Object, categoryRepositoryMock.Object);

            bool created = await handler.HandleAsync(10, 2);

            created.Should().BeFalse();
            productRepositoryMock.Verify(x => x.AddCategoryAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_Product_Does_Not_Exist()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();

            AddCategoryToProductHandler handler = new AddCategoryToProductHandler(productRepositoryMock.Object, categoryRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(10, 2);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Throw_BadRequest_When_Category_Is_Invalid()
        {
            Mock<IProductRepository> productRepositoryMock = new Mock<IProductRepository>();
            productRepositoryMock.Setup(x => x.ExistsActiveByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock.Setup(x => x.ExistsActiveByCodeAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            AddCategoryToProductHandler handler = new AddCategoryToProductHandler(productRepositoryMock.Object, categoryRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(10, 2);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Categoria inválida");
        }
    }
}
