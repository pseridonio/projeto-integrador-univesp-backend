using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class GetUserByIdHandlerTests
    {
        [Fact]
        public async Task Should_Return_User_When_User_Is_Active()
        {
            Guid userId = Guid.NewGuid();
            User user = BuildUser(userId);

            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetByIdNoTrackingAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            GetUserByIdHandler handler = new GetUserByIdHandler(repositoryMock.Object);

            GetUserResponse response = await handler.HandleAsync(userId);

            response.Code.Should().Be(userId);
            response.FullName.Should().Be(user.FullName);
            response.Email.Should().Be(user.Email);
            response.BirthDate.Should().Be("1990-01-01");
        }

        [Fact]
        public async Task Should_Throw_When_User_Does_Not_Exist()
        {
            Guid userId = Guid.NewGuid();
            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetByIdNoTrackingAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            GetUserByIdHandler handler = new GetUserByIdHandler(repositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(userId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Throw_When_User_Is_Inactive()
        {
            Guid userId = Guid.NewGuid();
            User user = BuildUser(userId);
            user.IsActive = false;

            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetByIdNoTrackingAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            GetUserByIdHandler handler = new GetUserByIdHandler(repositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(userId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_Throw_When_User_Is_Deleted()
        {
            Guid userId = Guid.NewGuid();
            User user = BuildUser(userId);
            user.DeletedAt = DateTime.UtcNow;

            Mock<IUserRepository> repositoryMock = new Mock<IUserRepository>();
            repositoryMock
                .Setup(x => x.GetByIdNoTrackingAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            GetUserByIdHandler handler = new GetUserByIdHandler(repositoryMock.Object);

            Func<Task> act = async () => await handler.HandleAsync(userId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        private static User BuildUser(Guid userId)
        {
            return new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Valid User",
                BirthDate = new DateOnly(1990, 1, 1),
                IsActive = true
            };
        }
    }
}
