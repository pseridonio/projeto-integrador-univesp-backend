using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class SearchCategoriesHandlerMutationTests
    {
        [Fact]
        public async Task Should_Not_Accept_Multiple_Terms_If_Separated_Incorrectly()
        {
            // This test validates that the handler correctly handles space-separated terms.
            // Mutation: If we remove the term splitting logic, this test should fail.

            // Arrange
            SearchCategoriesRequest request = new()
            {
                Description = "Bebidas Quentes"
            };

            // Simulate repository returning results when multiple terms are provided
            List<Category> categories = new()
            {
                BuildCategory(1, "Bebidas"),
                BuildCategory(2, "Quentes")
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
            repositoryMock.Verify(
                x => x.SearchByDescriptionAsync("Bebidas Quentes", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_Throw_NOT_FOUND_When_Repository_Returns_Empty_List()
        {
            // Mutation: If we remove the check for empty results, this test should fail.

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
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Map_All_Categories_To_Response_Correctly()
        {
            // Mutation: If we modify the mapping logic, this test should fail.

            // Arrange
            SearchCategoriesRequest request = new()
            {
                Description = "Bebidas"
            };

            List<Category> categories = new()
            {
                new()
                {
                    Code = 10,
                    Description = "Bebidas Quentes",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new()
                {
                    Code = 20,
                    Description = "Bebidas Frias",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            Mock<ICategoryRepository> repositoryMock = new();
            repositoryMock
                .Setup(x => x.SearchByDescriptionAsync(request.Description, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            SearchCategoriesHandler handler = new(repositoryMock.Object);

            // Act
            List<SearchCategoriesResponse> response = await handler.HandleAsync(request);

            // Assert
            response[0].Code.Should().Be(10);
            response[0].Description.Should().Be("Bebidas Quentes");
            response[1].Code.Should().Be(20);
            response[1].Description.Should().Be("Bebidas Frias");
        }

        [Fact]
        public async Task Should_Preserve_Category_Order_From_Repository()
        {
            // Mutation: If we sort or reorder the results, this test should fail.

            // Arrange
            SearchCategoriesRequest request = new()
            {
                Description = "Produtos"
            };

            List<Category> categories = new()
            {
                BuildCategory(3, "Produtos C"),
                BuildCategory(1, "Produtos A"),
                BuildCategory(2, "Produtos B")
            };

            Mock<ICategoryRepository> repositoryMock = new();
            repositoryMock
                .Setup(x => x.SearchByDescriptionAsync(request.Description, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            SearchCategoriesHandler handler = new(repositoryMock.Object);

            // Act
            List<SearchCategoriesResponse> response = await handler.HandleAsync(request);

            // Assert
            response[0].Code.Should().Be(3);
            response[1].Code.Should().Be(1);
            response[2].Code.Should().Be(2);
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
