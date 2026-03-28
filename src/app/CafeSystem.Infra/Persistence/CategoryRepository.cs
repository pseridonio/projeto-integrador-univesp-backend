using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Infra.Persistence
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _dbContext;

        public CategoryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _dbContext.Categories.AddAsync(category, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
