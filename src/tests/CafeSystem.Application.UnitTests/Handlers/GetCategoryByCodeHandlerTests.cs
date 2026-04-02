using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class GetCategoryByCodeHandlerTests
    {
        [Fact]
        public async Task Should_Return_Category_When_Category_Is_Active()
        {
            int code = 1;
            Category category = BuildCategory(code);

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            GetCategoryByCodeHandler handler = new GetCategoryByCodeHandler(categoryRepositoryMock.Object);

            GetCategoryResponse response = await handler.HandleAsync(code);

            response.Description.Should().Be(category.Description);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task Should_Throw_When_Category_Is_Not_Found(int code)
        {
            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            GetCategoryByCodeHandler handler = new GetCategoryByCodeHandler(categoryRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(code);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Throw_When_Category_Is_Inactive()
        {
            int code = 2;
            Category category = BuildCategory(code);
            category.IsActive = false;

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            GetCategoryByCodeHandler handler = new GetCategoryByCodeHandler(categoryRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(code);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Throw_When_Category_Is_Deleted()
        {
            int code = 3;
            Category category = BuildCategory(code);
            category.DeletedAt = DateTime.UtcNow;

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            GetCategoryByCodeHandler handler = new GetCategoryByCodeHandler(categoryRepositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(code);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        private static Category BuildCategory(int code)
        {
            return new Category
            {
                Code = code,
                Description = "Bebidas",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }
    }
}
