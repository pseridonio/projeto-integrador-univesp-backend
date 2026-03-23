namespace CafeSystem.Application.Interfaces
{
    /// <summary>
    /// Unit of Work abstraction to coordinate transactions across repositories.
    /// </summary>
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
