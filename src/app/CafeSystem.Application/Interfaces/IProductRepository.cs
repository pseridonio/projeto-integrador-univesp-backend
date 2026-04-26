using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> ExistsActiveByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

        Task<bool> ExistsActiveByBarcodeExceptIdAsync(string barcode, int productId, CancellationToken cancellationToken = default);

        Task<Product?> GetActiveByIdNoTrackingAsync(int id, CancellationToken cancellationToken = default);

        Task CreateAsync(Product product, CancellationToken cancellationToken = default);

        Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    }
}
