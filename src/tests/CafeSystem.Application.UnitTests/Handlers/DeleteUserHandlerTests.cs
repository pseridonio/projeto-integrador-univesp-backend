using System;
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
    public class DeleteUserHandlerTests
    {
        [Fact]
        public async Task Should_Throw_When_Trying_To_Delete_Self()
        {
            // Arrange
            Mock<IUserRepository> userRepo = new Mock<IUserRepository>();
            Mock<IRefreshTokenRepository> tokenRepo = new Mock<IRefreshTokenRepository>();
            Mock<IUnitOfWork> uow = new Mock<IUnitOfWork>();

            DeleteUserHandler handler = new DeleteUserHandler(userRepo.Object, tokenRepo.Object, uow.Object);

            Guid id = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await handler.HandleAsync(id, id);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Não é possível excluir a si mesmo");
        }

        [Fact]
        public async Task Should_Throw_NotFound_When_User_Does_Not_Exist()
        {
            // Arrange
            Mock<IUserRepository> userRepo = new Mock<IUserRepository>();
            Mock<IRefreshTokenRepository> tokenRepo = new Mock<IRefreshTokenRepository>();
            Mock<IUnitOfWork> uow = new Mock<IUnitOfWork>();

            userRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            DeleteUserHandler handler = new DeleteUserHandler(userRepo.Object, tokenRepo.Object, uow.Object);

            Guid id = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await handler.HandleAsync(id, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("NOT_FOUND");
        }

        [Fact]
        public async Task Should_NoOp_When_User_Already_Deleted()
        {
            // Arrange
            Mock<IUserRepository> userRepo = new Mock<IUserRepository>();
            Mock<IRefreshTokenRepository> tokenRepo = new Mock<IRefreshTokenRepository>();
            Mock<IUnitOfWork> uow = new Mock<IUnitOfWork>();

            User user = new User { Id = Guid.NewGuid(), IsActive = false };
            userRepo.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            DeleteUserHandler handler = new DeleteUserHandler(userRepo.Object, tokenRepo.Object, uow.Object);

            // Act
            await handler.HandleAsync(user.Id, Guid.NewGuid());

            // Assert
            userRepo.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            tokenRepo.Verify(x => x.RevokeAllForUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Should_Mark_Deleted_And_Revoke_Tokens_When_Success()
        {
            // Arrange
            Mock<IUserRepository> userRepo = new Mock<IUserRepository>();
            Mock<IRefreshTokenRepository> tokenRepo = new Mock<IRefreshTokenRepository>();
            Mock<IUnitOfWork> uow = new Mock<IUnitOfWork>();

            User user = new User { Id = Guid.NewGuid(), IsActive = true };
            userRepo.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            userRepo.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            tokenRepo.Setup(x => x.RevokeAllForUserAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);
            uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            DeleteUserHandler handler = new DeleteUserHandler(userRepo.Object, tokenRepo.Object, uow.Object);

            Guid actingUser = Guid.NewGuid();

            // Act
            await handler.HandleAsync(user.Id, actingUser);

            // Assert
            user.IsActive.Should().BeFalse();
            user.DeletedBy.Should().Be(actingUser);
            user.DeletedAt.Should().NotBeNull();

            userRepo.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
            tokenRepo.Verify(x => x.RevokeAllForUserAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
