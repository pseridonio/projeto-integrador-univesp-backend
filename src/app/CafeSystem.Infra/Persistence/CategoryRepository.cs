using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using System.Linq.Expressions;
using System.Reflection;
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
            string[] terms = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<Category> query = _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.IsActive && !x.DeletedAt.HasValue);

            if (terms.Length == 0)
            {
                return new List<Category>();
            }

            ParameterExpression parameter = Expression.Parameter(typeof(Category), "x");
            Expression? predicateBody = null;
            MethodInfo likeMethod = typeof(DbFunctionsExtensions).GetMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

            foreach (string term in terms)
            {
                string searchTerm = term;
                Expression likeExpression = Expression.Call(
                    likeMethod,
                    Expression.Constant(EF.Functions),
                    Expression.Property(parameter, nameof(Category.Description)),
                    Expression.Constant("%" + searchTerm + "%"));

                predicateBody = predicateBody is null
                    ? likeExpression
                    : Expression.OrElse(predicateBody, likeExpression);
            }

            Expression<Func<Category, bool>> predicate = Expression.Lambda<Func<Category, bool>>(predicateBody!, parameter);

            return await query.Where(predicate).ToListAsync(cancellationToken);
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

