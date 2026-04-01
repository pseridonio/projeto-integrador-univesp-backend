using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class DeleteCategoryHandlerTests
    {
        [Fact]
        public async Task Should_Delete_Category_When_Category_Is_Active()
        {
            int code = 1;
            Category category = BuildCategory(code, isActive: true, deletedAt: null);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            DeleteCategoryHandler handler = new DeleteCategoryHandler(categoryRepositoryMock.Object);

            await handler.HandleAsync(code);

            category.IsActive.Should().BeFalse();
            category.DeletedAt.Should().NotBeNull();
            category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Return_When_Category_Is_Already_Inactive()
        {
            int code = 1;
            Category category = BuildCategory(code, isActive: false, deletedAt: null);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            DeleteCategoryHandler handler = new DeleteCategoryHandler(categoryRepositoryMock.Object);

            await handler.HandleAsync(code);

            categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Should_Return_When_Category_Is_Already_Deleted()
        {
            int code = 1;
            Category category = BuildCategory(code, isActive: false, deletedAt: DateTime.UtcNow);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            DeleteCategoryHandler handler = new DeleteCategoryHandler(categoryRepositoryMock.Object);

            await handler.HandleAsync(code);

            categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Should_Throw_When_Category_Does_Not_Exist()
        {
            int code = 1;
            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            DeleteCategoryHandler handler = new DeleteCategoryHandler(categoryRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(code);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        private static Category BuildCategory(int code, bool isActive, DateTime? deletedAt)
        {
            return new Category
            {
                Code = code,
                Description = "Bebidas",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = isActive,
                DeletedAt = deletedAt
            };
        }
    }
}
