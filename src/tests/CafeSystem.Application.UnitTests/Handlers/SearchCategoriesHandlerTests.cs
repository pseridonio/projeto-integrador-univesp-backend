using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class SearchCategoriesHandlerTests
    {
        [Fact]
        public async Task Should_Return_Categories_When_Search_Finds_Results()
        {
            // Arrange
            SearchCategoriesRequest request = new()
            {
                Description = "Bebidas"
            };

            List<Category> categories = new()
            {
                BuildCategory(1, "Bebidas Quentes"),
                BuildCategory(2, "Bebidas Frias")
            };

            Mock<ICategoryRepository> repositoryMock = new();
            repositoryMock
                .Setup(x => x.SearchByDescriptionAsync(request.Description, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            SearchCategoriesHandler handler = new(repositoryMock.Object);

            // Act
            List<SearchCategoriesResponse> response = await handler.HandleAsync(request);

            // Assert
            response.Should().HaveCount(2);
            response[0].Code.Should().Be(1);
            response[0].Description.Should().Be("Bebidas Quentes");
            response[1].Code.Should().Be(2);
            response[1].Description.Should().Be("Bebidas Frias");
        }

        [Fact]
        public async Task Should_Return_Single_Category_When_Search_Finds_One_Result()
        {
            // Arrange
            SearchCategoriesRequest request = new()
            {
                Description = "Alimentos"
            };

            List<Category> categories = new()
            {
                BuildCategory(3, "Alimentos Perecíveis")
            };

            Mock<ICategoryRepository> repositoryMock = new();
            repositoryMock
                .Setup(x => x.SearchByDescriptionAsync(request.Description, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            SearchCategoriesHandler handler = new(repositoryMock.Object);

            // Act
            List<SearchCategoriesResponse> response = await handler.HandleAsync(request);

            // Assert
            response.Should().HaveCount(1);
            response[0].Code.Should().Be(3);
            response[0].Description.Should().Be("Alimentos Perecíveis");
        }

        [Fact]
        public async Task Should_Throw_When_No_Categories_Found()
        {
            // Arrange
            SearchCategoriesRequest request = new()
            {
                Description = "Inexistente"
            };

            Mock<ICategoryRepository> repositoryMock = new();
            repositoryMock
                .Setup(x => x.SearchByDescriptionAsync(request.Description, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Category>());

            SearchCategoriesHandler handler = new(repositoryMock.Object);

            // Act
            Func<Task> act = async () => await handler.HandleAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Return_Categories_With_Multiple_Terms()
        {
            // Arrange
            SearchCategoriesRequest request = new()
            {
                Description = "Bebidas Alimentos"
            };

            List<Category> categories = new()
            {
                BuildCategory(1, "Bebidas Quentes"),
                BuildCategory(3, "Alimentos Perecíveis")
            };

            Mock<ICategoryRepository> repositoryMock = new();
            repositoryMock
                .Setup(x => x.SearchByDescriptionAsync(request.Description, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            SearchCategoriesHandler handler = new(repositoryMock.Object);

            // Act
            List<SearchCategoriesResponse> response = await handler.HandleAsync(request);

            // Assert
            response.Should().HaveCount(2);
            response[0].Description.Should().Be("Bebidas Quentes");
            response[1].Description.Should().Be("Alimentos Perecíveis");
        }

        [Fact]
        public async Task Should_Call_Repository_With_Correct_Description()
        {
            // Arrange
            string searchTerm = "Café";
            SearchCategoriesRequest request = new()
            {
                Description = searchTerm
            };

            List<Category> categories = new()
            {
                BuildCategory(4, "Café Arábica")
            };

            Mock<ICategoryRepository> repositoryMock = new();
            repositoryMock
                .Setup(x => x.SearchByDescriptionAsync(searchTerm, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            SearchCategoriesHandler handler = new(repositoryMock.Object);

            // Act
            await handler.HandleAsync(request);

            // Assert
            repositoryMock.Verify(
                x => x.SearchByDescriptionAsync(searchTerm, It.IsAny<CancellationToken>()),
                Times.Once);
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
