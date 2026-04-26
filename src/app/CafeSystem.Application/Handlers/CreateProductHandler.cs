using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class CreateProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateProductHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<Product> HandleAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            bool barcodeInUse = await _productRepository.ExistsActiveByBarcodeAsync(request.Barcode, cancellationToken);
            if (barcodeInUse)
            {
                throw new ArgumentException("Código de barras já utilizado.");
            }

            IReadOnlyCollection<int> distinctCategoryCodes = request.Categories.Distinct().ToList();
            int activeCategoriesCount = await _categoryRepository.CountActiveByCodesAsync(distinctCategoryCodes, cancellationToken);
            if (activeCategoriesCount != distinctCategoryCodes.Count)
            {
                throw new ArgumentException("Categoria inválida");
            }

            DateTime currentDateTime = DateTime.UtcNow;
            Product product = new Product
            {
                Barcode = request.Barcode.Trim(),
                Description = request.Description.Trim(),
                UnitPrice = request.UnitPrice,
                IsDeleted = false,
                CreatedAt = currentDateTime,
                UpdatedAt = currentDateTime
            };

            foreach (int categoryCode in distinctCategoryCodes)
            {
                ProductCategory productCategory = new ProductCategory
                {
                    CategoryCode = categoryCode,
                    Product = product
                };

                product.ProductCategories.Add(productCategory);
            }

            await _productRepository.CreateAsync(product, cancellationToken);

            return product;
        }
    }
}
