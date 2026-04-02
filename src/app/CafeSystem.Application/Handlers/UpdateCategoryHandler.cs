using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Application.Validation;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class UpdateCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public UpdateCategoryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> HandleAsync(int code, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            Category? category = await _categoryRepository.GetByCodeNoTrackingAsync(code, cancellationToken);
            if (category == null)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            if (!CategoryValidationHelper.IsValidDescription(request.Description))
            {
                throw new ArgumentException("Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números");
            }

            category.Description = CategoryValidationHelper.NormalizeDescription(request.Description);
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category, cancellationToken);

            return category;
        }
    }
}
