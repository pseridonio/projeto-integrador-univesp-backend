using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByCodeNoTrackingAsync(int code, CancellationToken cancellationToken = default);

        Task<List<Category>> SearchByDescriptionAsync(string description, CancellationToken cancellationToken = default);

        Task CreateAsync(Category category, CancellationToken cancellationToken = default);

        Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

        Task<int> CountActiveByCodesAsync(IReadOnlyCollection<int> codes, CancellationToken cancellationToken = default);
    }
}
