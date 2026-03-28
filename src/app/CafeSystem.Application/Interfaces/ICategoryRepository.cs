using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task CreateAsync(Category category, CancellationToken cancellationToken = default);
    }
}
