using System;
using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por renovar token de acesso a partir de um refresh token válido.
    /// </summary>
    public class RefreshTokenHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public RefreshTokenHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            ITokenService tokenService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<RefreshTokenResponse?> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            RefreshToken? storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (storedRefreshToken == null || !storedRefreshToken.IsActive)
            {
                return null;
            }

            RefreshToken? storedAccessToken = await _refreshTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
            if (storedAccessToken == null || !storedAccessToken.IsActive || storedAccessToken.UserId != storedRefreshToken.UserId)
            {
                return null;
            }

            User? user = await _userRepository.GetByIdAsync(storedRefreshToken.UserId, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            string newAccessToken = _tokenService.GenerateAccessToken(user);
            int accessExpiresInMinutes = _tokenService.GetAccessTokenExpiryMinutes();
            DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(accessExpiresInMinutes);

            RefreshToken newAccessTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newAccessToken,
                UserId = user.Id,
                ExpiresAt = expiresAtUtc
            };

            await _refreshTokenRepository.CreateAsync(newAccessTokenEntity, cancellationToken);

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                ExpiresAtUtc = expiresAtUtc
            };
        }
    }
}
