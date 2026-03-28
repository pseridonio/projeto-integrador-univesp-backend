using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class GetUsersHandlerTests
    {
        [Fact]
        public async Task Should_Return_Users_When_Repository_Finds_Matches()
        {
            // Arrange
            Guid firstUserId = Guid.NewGuid();
            Guid secondUserId = Guid.NewGuid();

            List<User> users = new List<User>
            {
                new User
                {
                    Id = firstUserId,
                    FullName = "Maria Jose Inacio",
                    Email = "maria.jose@example.com",
                    BirthDate = new DateOnly(1985, 4, 10)
                },
                new User
                {
                    Id = secondUserId,
                    FullName = "Jose Maria Souza",
                    Email = "jose.maria@example.com",
                    BirthDate = null
                }
            };

            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetListAsync(
                    It.Is<IReadOnlyCollection<string>>(terms => terms.SequenceEqual(new[] { "Maria", "Jose" })),
                    It.Is<IReadOnlyCollection<string>>(terms => terms.SequenceEqual(new[] { "example.com" })),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            GetUsersHandler handler = new GetUsersHandler(repositoryMock.Object);

            // Act
            IReadOnlyList<CafeSystem.Application.DTOs.GetUserResponse> response = await handler.HandleAsync(new[] { "Maria", "Jose" }, new[] { "example.com" });

            // Assert
            response.Should().HaveCount(2);
            response[0].Code.Should().Be(firstUserId);
            response[0].FullName.Should().Be("Maria Jose Inacio");
            response[0].Email.Should().Be("maria.jose@example.com");
            response[0].BirthDate.Should().Be("1985-04-10");
            response[1].Code.Should().Be(secondUserId);
            response[1].BirthDate.Should().BeNull();

            repositoryMock.VerifyAll();
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_Repository_Returns_Empty_List()
        {
            // Arrange
            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetListAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<User>());

            GetUsersHandler handler = new GetUsersHandler(repositoryMock.Object);

            // Act
            Func<Task> act = async () => await handler.HandleAsync(Array.Empty<string>(), Array.Empty<string>());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Normalize_Query_Terms_Before_Calling_Repository()
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
            IReadOnlyList<CafeSystem.Application.DTOs.GetUserResponse> response = await handler.HandleAsync(new[] { "  Maria , Jose  " }, new[] { " example , domain " });

            // Assert
            response.Should().HaveCount(1);
            repositoryMock.VerifyAll();
        }
    }
}
