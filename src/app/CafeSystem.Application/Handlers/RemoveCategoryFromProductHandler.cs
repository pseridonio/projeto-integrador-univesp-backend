using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class RemoveCategoryFromProductHandler
    {
        private readonly IProductRepository _productRepository;

        public RemoveCategoryFromProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(int productId, int categoryId, CancellationToken cancellationToken = default)
        {
            bool productExists = await _productRepository.ExistsActiveByIdAsync(productId, cancellationToken);
            if (!productExists)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            bool associationExists = await _productRepository.ExistsCategoryAssociationAsync(productId, categoryId, cancellationToken);
            if (!associationExists)
            {
                throw new ArgumentException("Categoria inválida");
            }

            int categoryAssociationsCount = await _productRepository.CountCategoryAssociationsAsync(productId, cancellationToken);
            if (categoryAssociationsCount <= 1)
            {
                throw new ArgumentException("O produto deve ter ao menos 1 categoria");
            }

            ProductCategory productCategory = new ProductCategory
            {
                ProductId = productId,
                CategoryCode = categoryId
            };

            await _productRepository.RemoveCategoryAsync(productCategory, cancellationToken);
        }
    }
}
