using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class AddCategoryToProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public AddCategoryToProductHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<bool> HandleAsync(int productId, int categoryId, CancellationToken cancellationToken = default)
        {
            bool productExists = await _productRepository.ExistsActiveByIdAsync(productId, cancellationToken);
            if (!productExists)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            bool categoryExists = await _categoryRepository.ExistsActiveByCodeAsync(categoryId, cancellationToken);
            if (!categoryExists)
            {
                throw new ArgumentException("Categoria inválida");
            }

            bool associationExists = await _productRepository.ExistsCategoryAssociationAsync(productId, categoryId, cancellationToken);
            if (associationExists)
            {
                return false;
            }

            ProductCategory productCategory = new ProductCategory
            {
                ProductId = productId,
                CategoryCode = categoryId
            };

            await _productRepository.AddCategoryAsync(productCategory, cancellationToken);
            return true;
        }
    }
}
