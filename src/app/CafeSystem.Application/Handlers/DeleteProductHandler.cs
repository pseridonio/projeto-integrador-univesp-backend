using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por excluir produtos de forma lógica.
    /// </summary>
    public class DeleteProductHandler
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(int id, CancellationToken cancellationToken = default)
        {
            Product? product = await _productRepository.GetActiveByIdNoTrackingAsync(id, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            DateTime currentDateTime = DateTime.UtcNow;
            product.IsDeleted = true;
            product.Barcode = string.Empty;
            product.DeletedAt = currentDateTime;
            product.UpdatedAt = currentDateTime;

            await _productRepository.UpdateAsync(product, cancellationToken);
        }
    }
}
