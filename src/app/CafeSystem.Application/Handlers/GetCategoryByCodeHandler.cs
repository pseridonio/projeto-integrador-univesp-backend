using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class GetCategoryByCodeHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoryByCodeHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<GetCategoryResponse> HandleAsync(int code, CancellationToken cancellationToken = default)
        {
            Category? category = await _categoryRepository.GetByCodeNoTrackingAsync(code, cancellationToken);
            if (category == null || !category.IsActive || category.DeletedAt.HasValue)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            return new GetCategoryResponse
            {
                Description = category.Description
            };
        }
    }
}
