using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Application.DTOs;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class LoginHandlerTests
    {
        [Fact]
        public async Task Should_Return_Null_When_User_Not_Found()
        {
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<ITokenService> tokenServiceMock = new Mock<ITokenService>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            Mock<IRefreshTokenRepository> refreshRepoMock = new Mock<IRefreshTokenRepository>();

            userRepositoryMock
                .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            LoginHandler handler = new LoginHandler(userRepositoryMock.Object, tokenServiceMock.Object, passwordHasherMock.Object, refreshRepoMock.Object);

            LoginResponse? response = await handler.HandleAsync(new LoginRequest { Email = "noone@example.com", Password = "pwd" });

            response.Should().BeNull();
        }

        [Fact]
        public async Task Should_Persist_Access_And_Refresh_Tokens_When_Login_Is_Valid()
        {
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<ITokenService> tokenServiceMock = new Mock<ITokenService>();
            Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
            Mock<IRefreshTokenRepository> refreshRepoMock = new Mock<IRefreshTokenRepository>();

            User user = new User
            {
                Id = System.Guid.NewGuid(),
                Email = "john@example.com",
                FullName = "John",
                PasswordHash = "hashed-password",
                PasswordSalt = "salt",
                IsActive = true
            };

            userRepositoryMock
                .Setup(x => x.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            passwordHasherMock
                .Setup(x => x.Verify("hashed-password", "salt", "secret"))
                .Returns(true);

            tokenServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("access-token");

            tokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            tokenServiceMock
                .Setup(x => x.GetAccessTokenExpiryMinutes())
                .Returns(60);

            tokenServiceMock
                .Setup(x => x.GetRefreshTokenExpiryMinutes())
                .Returns(1440);

            LoginHandler handler = new LoginHandler(userRepositoryMock.Object, tokenServiceMock.Object, passwordHasherMock.Object, refreshRepoMock.Object);

            LoginResponse? response = await handler.HandleAsync(new LoginRequest
            {
                Email = "john@example.com",
                Password = "secret"
            });

            response.Should().NotBeNull();
            response!.AccessToken.Should().Be("access-token");
            response.RefreshToken.Should().Be("refresh-token");

            refreshRepoMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
