using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por processar o comando de login.
    /// </summary>
    public class LoginHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginHandler(IUserRepository userRepository, ITokenService tokenService, IPasswordHasher passwordHasher, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResponse?> HandleAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            CafeSystem.Domain.Entities.User? user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user == null || !user.IsActive)
                return null;

            if (!_passwordHasher.Verify(user.PasswordHash, user.PasswordSalt, request.Password))
                return null;

            string accessToken = _tokenService.GenerateAccessToken(user);
            int accessExpiresMinutes = _tokenService.GetAccessTokenExpiryMinutes();
            string refreshToken = _tokenService.GenerateRefreshToken();
            int refreshExpiresMinutes = _tokenService.GetRefreshTokenExpiryMinutes();
            System.DateTime utcNow = System.DateTime.UtcNow;

            RefreshToken accessTokenEntity = new RefreshToken
            {
                Id = System.Guid.NewGuid(),
                Token = accessToken,
                UserId = user.Id,
                ExpiresAt = utcNow.AddMinutes(accessExpiresMinutes)
            };

            RefreshToken refreshEntity = new RefreshToken
            {
                Id = System.Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = utcNow.AddMinutes(refreshExpiresMinutes)
            };

            await _refreshTokenRepository.CreateAsync(accessTokenEntity, cancellationToken);
            await _refreshTokenRepository.CreateAsync(refreshEntity, cancellationToken);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = accessExpiresMinutes * 60,
                UserId = user.Id,
                UserName = user.FullName,
                Roles = user.Roles
            };
        }
    }
}
