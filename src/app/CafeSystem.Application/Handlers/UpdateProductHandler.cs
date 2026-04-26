using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class UpdateProductHandler
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product> HandleAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            string normalizedBarcode = request.Barcode.Trim();
            string normalizedDescription = request.Description.Trim();

            Product? product = await _productRepository.GetActiveByIdNoTrackingAsync(id, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            bool barcodeInUseByOtherProduct = await _productRepository.ExistsActiveByBarcodeExceptIdAsync(normalizedBarcode, id, cancellationToken);
            if (barcodeInUseByOtherProduct)
            {
                throw new ArgumentException("Código de barras já utilizado.");
            }

            product.Barcode = normalizedBarcode;
            product.Description = normalizedDescription;
            product.UnitPrice = request.UnitPrice;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product, cancellationToken);

            return product;
        }
    }
}
