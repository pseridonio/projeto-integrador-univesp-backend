using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CafeSystem.Application.UnitTests.Handlers
{
    public class RefreshTokenHandlerTests
    {
        [Fact]
        public async Task Should_Return_Null_When_Refresh_Token_Is_Invalid()
        {
            Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<ITokenService> tokenServiceMock = new Mock<ITokenService>();

            refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("invalid-refresh", It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            RefreshTokenHandler handler = new RefreshTokenHandler(
                refreshTokenRepositoryMock.Object,
                userRepositoryMock.Object,
                tokenServiceMock.Object);

            RefreshTokenResponse? response = await handler.HandleAsync(new RefreshTokenRequest
            {
                Token = "access-token",
                RefreshToken = "invalid-refresh"
            });

            response.Should().BeNull();
        }

        [Fact]
        public async Task Should_Generate_And_Persist_New_Access_Token_When_Refresh_Is_Valid()
        {
            Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            Mock<ITokenService> tokenServiceMock = new Mock<ITokenService>();

            Guid userId = Guid.NewGuid();

            RefreshToken activeRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "refresh-token",
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };

            RefreshToken activeAccessToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "current-access-token",
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            User user = new User
            {
                Id = userId,
                Email = "john@example.com",
                FullName = "John",
                IsActive = true
            };

            refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRefreshToken);

            refreshTokenRepositoryMock
                .Setup(x => x.GetByTokenAsync("current-access-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeAccessToken);

            userRepositoryMock
                .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            tokenServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .Returns("new-access-token");

            tokenServiceMock
                .Setup(x => x.GetAccessTokenExpiryMinutes())
                .Returns(60);

            RefreshTokenHandler handler = new RefreshTokenHandler(
                refreshTokenRepositoryMock.Object,
                userRepositoryMock.Object,
                tokenServiceMock.Object);

            RefreshTokenResponse? response = await handler.HandleAsync(new RefreshTokenRequest
            {
                Token = "current-access-token",
                RefreshToken = "refresh-token"
            });

            response.Should().NotBeNull();
            response!.AccessToken.Should().Be("new-access-token");
            response.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);

            refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
