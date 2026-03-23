using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeSystem.Infra.Persistence
{
    /// <summary>
    /// Implementação do repositório de refresh tokens.
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _dbContext;

        public RefreshTokenRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<RefreshToken>().AddAsync(token, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            RefreshToken? found = await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
            return found;
        }

        public async Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            token.RevokedAt = DateTime.UtcNow;
            _dbContext.Set<RefreshToken>().Update(token);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _dbContext.Set<RefreshToken>().Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(cancellationToken);
            foreach (var t in tokens)
            {
                t.RevokedAt = DateTime.UtcNow;
            }
            if (tokens.Count > 0)
            {
                _dbContext.Set<RefreshToken>().UpdateRange(tokens);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return tokens.Count;
        }
    }
}
