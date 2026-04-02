using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class CreateCategoryHandlerTests
    {
        [Fact]
        public async Task Should_Create_Category_When_Description_Is_Valid()
        {
            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            CreateCategoryHandler handler = new CreateCategoryHandler(categoryRepositoryMock.Object);

            CreateCategoryRequest request = new CreateCategoryRequest
            {
                Description = "Bebidas"
            };

            Category category = await handler.HandleAsync(request);

            category.Description.Should().Be("Bebidas");
            category.IsActive.Should().BeTrue();
            categoryRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Abcd")]
        [InlineData("Categoria#1")]
        public async Task Should_Throw_Exception_When_Description_Is_Invalid(string? description)
        {
            Mock<ICategoryRepository> categoryRepositoryMock = new Mock<ICategoryRepository>();
            CreateCategoryHandler handler = new CreateCategoryHandler(categoryRepositoryMock.Object);

            CreateCategoryRequest request = new CreateCategoryRequest
            {
                Description = description ?? string.Empty
            };

            System.Func<Task> act = async () => await handler.HandleAsync(request);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números");
        }
    }
}
