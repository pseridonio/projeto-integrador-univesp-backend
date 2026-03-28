using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class ChangePasswordHandlerTests
    {
        [Fact]
        public async Task Should_Update_Password_When_Request_Is_Valid()
        {
            // Arrange
            Guid userId = Guid.NewGuid();

            User user = new User
            {
                Id = userId,
                Email = "john@example.com",
                FullName = "John Doe",
                PasswordHash = "old-hash",
                PasswordSalt = "old-salt",
                IsActive = true
            };

            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            userRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            passwordHasherMock
                .Setup(x => x.Hash("newSecret1"))
                .Returns(new PasswordHashResult
                {
                    Hash = "new-hash",
                    Salt = "new-salt"
                });

            ChangePasswordHandler handler = new ChangePasswordHandler(userRepositoryMock.Object, passwordHasherMock.Object);

            ChangePasswordRequest request = new ChangePasswordRequest
            {
                Password = "newSecret1"
            };

            // Act
            User result = await handler.HandleAsync(userId, request);

            // Assert
            result.PasswordHash.Should().Be("new-hash");
            result.PasswordSalt.Should().Be("new-salt");
            result.UpdatedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
            userRepositoryMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
