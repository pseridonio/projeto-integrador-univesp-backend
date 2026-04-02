using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class GetUsersHandlerMutationTests
    {
        [Theory]
        [InlineData("Maria", "example.com", 1)]
        [InlineData("Maria|Jose", "example.com", 1)]
        [InlineData("Maria|Jose", "contoso.com", 0)]
        public async Task Should_Respect_Name_And_Email_Filter_Combinations(string nameTermsCsv, string emailTermsCsv, int expectedCount)
        {
            // Arrange
            string[] nameTerms = nameTermsCsv.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string[] emailTerms = emailTermsCsv.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            List<User> users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Maria Jose Inacio",
                    Email = "maria.jose@example.com",
                    BirthDate = new DateOnly(1990, 1, 1)
                }
            };

            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetListAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCount > 0 ? users : Array.Empty<User>());

            GetUsersHandler handler = new GetUsersHandler(repositoryMock.Object);

            if (expectedCount == 0)
            {
                Func<Task> act = async () => await handler.HandleAsync(nameTerms, emailTerms);
                await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
                return;
            }

            // Act
            IReadOnlyList<CafeSystem.Application.DTOs.GetUserResponse> response = await handler.HandleAsync(nameTerms, emailTerms);

            // Assert
            response.Should().HaveCount(1);
            response[0].FullName.Should().Be("Maria Jose Inacio");
        }

        [Fact]
        public async Task Should_Ignore_Blank_Terms_When_Normalizing_Query()
        {
            // Arrange
            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetListAsync(
                    It.Is<IReadOnlyCollection<string>>(terms => terms.SequenceEqual(new[] { "Maria", "Jose" })),
                    It.Is<IReadOnlyCollection<string>>(terms => terms.SequenceEqual(new[] { "example", "domain" })),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>
                {
                    new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Maria Jose Inacio",
                        Email = "maria@domain.example",
                        BirthDate = new DateOnly(1990, 1, 1)
                    }
                });

            GetUsersHandler handler = new GetUsersHandler(repositoryMock.Object);

            // Act
            IReadOnlyList<CafeSystem.Application.DTOs.GetUserResponse> response = await handler.HandleAsync(new[] { "", " Maria , , Jose " }, new[] { "   ", "example, domain" });

            // Assert
            response.Should().HaveCount(1);
            repositoryMock.VerifyAll();
        }
    }
}
