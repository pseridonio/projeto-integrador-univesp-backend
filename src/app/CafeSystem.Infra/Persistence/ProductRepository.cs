using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
    }
}
