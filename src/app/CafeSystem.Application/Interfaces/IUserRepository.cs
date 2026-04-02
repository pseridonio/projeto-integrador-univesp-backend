using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Interfaces
{
    /// <summary>
    /// Repositório de usuários. Define operações necessárias para a camada de aplicação.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<User?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<User>> GetListAsync(IReadOnlyCollection<string> nameTerms, IReadOnlyCollection<string> emailTerms, CancellationToken cancellationToken = default);

        Task CreateAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
