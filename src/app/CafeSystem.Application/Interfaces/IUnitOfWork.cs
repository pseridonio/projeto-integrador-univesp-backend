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

        /// <summary>
        /// Execute the provided action inside a database transaction using the underlying
        /// provider execution strategy. This ensures compatibility with providers that
        /// implement retry strategies (eg. Npgsql) and guarantees commit/rollback semantics
        /// for the provided action.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    }
}
