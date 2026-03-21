using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class RegisterHandlerTests
    {
        [Fact]
        public async Task Should_Create_User_When_Email_Not_Exists()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var passwordHasherMock = new Mock<IPasswordHasher>();

            userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            passwordHasherMock
                .Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("hashed");

            var handler = new RegisterHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            // Act
            User user = await handler.HandleAsync("test@example.com", "password", "Test User");

            // Assert
            user.Email.Should().Be("test@example.com");
            user.PasswordHash.Should().Be("hashed");
            userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
