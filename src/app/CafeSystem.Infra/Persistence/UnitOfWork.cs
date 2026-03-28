using CafeSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CafeSystem.Infra.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async ct =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    await action(ct);
                    await _dbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            }, cancellationToken);
        }
    }
}
