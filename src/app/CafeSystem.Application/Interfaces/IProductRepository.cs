using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> ExistsActiveByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

        Task CreateAsync(Product product, CancellationToken cancellationToken = default);
    }
}
