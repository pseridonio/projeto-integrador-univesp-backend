using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CafeSystem.Infra.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _dbContext;

        public ProductRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsActiveByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.Barcode == barcode, cancellationToken);
        }

        public async Task<bool> ExistsActiveByBarcodeExceptIdAsync(string barcode, int productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.Barcode == barcode && x.Id != productId, cancellationToken);
        }

        public async Task<bool> ExistsActiveByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsCategoryAssociationAsync(int productId, int categoryCode, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProductCategories
                .AsNoTracking()
                .AnyAsync(x => x.ProductId == productId && x.CategoryCode == categoryCode, cancellationToken);
        }

        public async Task<int> CountCategoryAssociationsAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProductCategories
                .AsNoTracking()
                .CountAsync(x => x.ProductId == productId, cancellationToken);
        }

        public async Task<List<Product>> SearchAsync(int? id, string? description, int? categoryId, string? barcode, bool includeCategories, string? sort, CancellationToken cancellationToken = default)
        {
            IQueryable<Product> query = _dbContext.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (id.HasValue)
            {
                query = query.Where(x => x.Id == id.Value);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                string[] terms = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (string term in terms)
                {
                    string searchTerm = term;
                    query = query.Where(x => EF.Functions.Like(x.Description, $"%{searchTerm}%"));
                }
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.ProductCategories.Any(pc => pc.CategoryCode == categoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(barcode))
            {
                query = query.Where(x => x.Barcode == barcode);
            }

            if (includeCategories)
            {
                query = query.Include(x => x.ProductCategories)
                    .ThenInclude(x => x.Category);
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                bool descending = sort.StartsWith('-');
                string normalizedSort = descending ? sort[1..] : sort;

                query = normalizedSort switch
                {
                    "id" => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                    "barcode" => descending ? query.OrderByDescending(x => x.Barcode) : query.OrderBy(x => x.Barcode),
                    "description" => descending ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                    "unitPrice" => descending ? query.OrderByDescending(x => x.UnitPrice) : query.OrderBy(x => x.UnitPrice),
                    "createdAt" => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                    "updatedAt" => descending ? query.OrderByDescending(x => x.UpdatedAt) : query.OrderBy(x => x.UpdatedAt),
                    _ => query
                };
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetActiveByIdNoTrackingAsync(int id, CancellationToken cancellationToken = default)
        {
            Product? product = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            return product;
        }

        public async Task CreateAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _dbContext.Products.AddAsync(product, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            await _dbContext.ProductCategories.AddAsync(productCategory, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            _dbContext.ProductCategories.Remove(productCategory);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
