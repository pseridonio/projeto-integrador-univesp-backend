using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeSystem.Infra.Persistence
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _dbContext;

        public CategoryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Category?> GetByCodeNoTrackingAsync(int code, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        }

        public async Task<List<Category>> SearchByDescriptionAsync(string description, CancellationToken cancellationToken = default)
        {
            string[] terms = description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            List<Category> categories = await _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.IsActive && !x.DeletedAt.HasValue)
                .Where(x => terms.Any(term => EF.Functions.Like(x.Description, $"%{term}%")))
                .ToListAsync(cancellationToken);

            return categories;
        }

        public async Task CreateAsync(Category category, CancellationToken cancellationToken = default)
        {
            await _dbContext.Categories.AddAsync(category, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CountActiveByCodesAsync(IReadOnlyCollection<int> codes, CancellationToken cancellationToken = default)
        {
            if (codes.Count == 0)
            {
                return 0;
            }

            return await _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.IsActive && !x.DeletedAt.HasValue)
                .CountAsync(x => codes.Contains(x.Code), cancellationToken);
        }
    }
}

