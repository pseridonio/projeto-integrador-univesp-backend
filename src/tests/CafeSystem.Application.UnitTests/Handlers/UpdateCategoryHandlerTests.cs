using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class UpdateCategoryHandlerTests
    {
        [Fact]
        public async Task Should_Update_Category_When_Request_Is_Valid()
        {
            int code = 1;
            Category category = BuildCategory(code, "Bebidas");

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            UpdateCategoryHandler handler = new UpdateCategoryHandler(categoryRepositoryMock.Object);
            UpdateCategoryRequest request = new UpdateCategoryRequest
            {
                Description = "Cafés"
            };

            Category updatedCategory = await handler.HandleAsync(code, request);

            updatedCategory.Description.Should().Be("Cafés");
            updatedCategory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(null, "Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números")]
        [InlineData("", "Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números")]
        [InlineData("   ", "Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números")]
        [InlineData("Abcd", "Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números")]
        [InlineData("Categoria#1", "Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números")]
        public async Task Should_Fail_When_Description_Is_Invalid(string? description, string expectedMessage)
        {
            int code = 1;
            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildCategory(code, "Bebidas"));

            UpdateCategoryHandler handler = new UpdateCategoryHandler(categoryRepositoryMock.Object);
            UpdateCategoryRequest request = new UpdateCategoryRequest
            {
                Description = description ?? string.Empty
            };

            Func<Task> act = async () => await handler.HandleAsync(code, request);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage(expectedMessage);
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public async Task Should_Update_Category_When_Description_Is_At_Boundary_Length(string description)
        {
            int code = 1;
            Category category = BuildCategory(code, "Bebidas");

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            UpdateCategoryHandler handler = new UpdateCategoryHandler(categoryRepositoryMock.Object);
            UpdateCategoryRequest request = new UpdateCategoryRequest
            {
                Description = description
            };

            Category updatedCategory = await handler.HandleAsync(code, request);

            updatedCategory.Description.Should().Be(description);
            categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Update_Category_When_New_Description_Is_Equal_To_Current_Description()
        {
            int code = 1;
            Category category = BuildCategory(code, "Bebidas");

            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            UpdateCategoryHandler handler = new UpdateCategoryHandler(categoryRepositoryMock.Object);
            UpdateCategoryRequest request = new UpdateCategoryRequest
            {
                Description = "Bebidas"
            };

            Category updatedCategory = await handler.HandleAsync(code, request);

            updatedCategory.Description.Should().Be("Bebidas");
            categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_When_Category_Does_Not_Exist()
        {
            int code = 1;
            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            categoryRepositoryMock
                .Setup(x => x.GetByCodeNoTrackingAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            UpdateCategoryHandler handler = new UpdateCategoryHandler(categoryRepositoryMock.Object);
            UpdateCategoryRequest request = new UpdateCategoryRequest
            {
                Description = "Cafés"
            };

            Func<Task> act = async () => await handler.HandleAsync(code, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        private static Category BuildCategory(int code, string description)
        {
            return new Category
            {
                Code = code,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }
    }
}
