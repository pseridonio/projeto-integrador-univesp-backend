using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class UpdateUserHandlerTests
    {
        [Fact]
        public async Task Should_Update_User_When_Request_Is_Valid()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            User user = new User
            {
                Id = userId,
                FullName = "Old Name",
                BirthDate = new DateOnly(1990, 1, 1)
            };

            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            UpdateUserHandler handler = new UpdateUserHandler(userRepositoryMock.Object);
            UpdateUserRequest request = new UpdateUserRequest
            {
                FullName = "Updated Name",
                BirthDate = "1991-02-10"
            };

            // Act
            User updatedUser = await handler.HandleAsync(userId, request);

            // Assert
            updatedUser.FullName.Should().Be("Updated Name");
            updatedUser.BirthDate.Should().Be(new DateOnly(1991, 2, 10));
            userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(null, "O campo nome é obrigatório")]
        [InlineData("", "O campo nome é obrigatório")]
        [InlineData("   ", "O campo nome é obrigatório")]
        [InlineData("Abcd", "O campo nome deve conter 5 ou mais caracteres")]
        public async Task Should_Fail_When_FullName_Is_Invalid(string? fullName, string expectedMessage)
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            UpdateUserHandler handler = new UpdateUserHandler(userRepositoryMock.Object);

            UpdateUserRequest request = new UpdateUserRequest
            {
                FullName = fullName ?? string.Empty,
                BirthDate = "1990-01-01"
            };

            // Act
            Func<Task> act = async () => await handler.HandleAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public async Task Should_Fail_When_FullName_Has_More_Than_250_Characters()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            UpdateUserHandler handler = new UpdateUserHandler(userRepositoryMock.Object);

            UpdateUserRequest request = new UpdateUserRequest
            {
                FullName = new string('a', 251),
                BirthDate = "1990-01-01"
            };

            // Act
            Func<Task> act = async () => await handler.HandleAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("O campo nome deve conter no máximo 250 caracteres.");
        }

        [Theory]
        [InlineData("3000-01-01")]
        [InlineData("2023-02-29")]
        [InlineData("31/01/2020")]
        [InlineData("2024-13-10")]
        public async Task Should_Fail_When_BirthDate_Is_Invalid(string birthDate)
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            UpdateUserHandler handler = new UpdateUserHandler(userRepositoryMock.Object);

            UpdateUserRequest request = new UpdateUserRequest
            {
                FullName = "Valid Name",
                BirthDate = birthDate
            };

            // Act
            Func<Task> act = async () => await handler.HandleAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Data de nascimento inválida.");
        }

        [Fact]
        public async Task Should_Clear_BirthDate_When_Not_Informed()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            User user = new User
            {
                Id = userId,
                FullName = "Old Name",
                BirthDate = new DateOnly(1995, 1, 1)
            };

            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            UpdateUserHandler handler = new UpdateUserHandler(userRepositoryMock.Object);
            UpdateUserRequest request = new UpdateUserRequest
            {
                FullName = "Updated Name",
                BirthDate = null
            };

            // Act
            User updatedUser = await handler.HandleAsync(userId, request);

            // Assert
            updatedUser.BirthDate.Should().BeNull();
            userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
