using System;
using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Interfaces
{
    /// <summary>
    /// Repositório para persistência de refresh tokens.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);

        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

        Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default);
    }
}
