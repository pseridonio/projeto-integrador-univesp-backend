using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class SearchProductsHandler
    {
        private readonly IProductRepository _productRepository;

        public SearchProductsHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<SearchProductsResponse>> HandleAsync(SearchProductsRequest request, CancellationToken cancellationToken = default)
        {
            List<Product> products = await _productRepository.SearchAsync(
                request.Id,
                request.Description,
                request.CategoryId,
                request.Barcode,
                request.IncludeCategories,
                request.Sort,
                cancellationToken);

            if (products.Count == 0)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            return products.Select(x => new SearchProductsResponse
            {
                Id = x.Id,
                Barcode = x.Barcode,
                Description = x.Description,
                UnitPrice = x.UnitPrice,
                Categories = request.IncludeCategories
                    ? x.ProductCategories
                        .Select(category => new SearchProductsCategoryResponse
                        {
                            Code = category.CategoryCode,
                            Description = category.Category.Description
                        })
                        .ToList()
                    : null
            }).ToList();
        }
    }
}
