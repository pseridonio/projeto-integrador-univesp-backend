using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Application.Validation;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class CreateCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> HandleAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (!CategoryValidationHelper.IsValidDescription(request.Description))
            {
                throw new ArgumentException("Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números");
            }

            Category category = new Category
            {
                Description = CategoryValidationHelper.NormalizeDescription(request.Description),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _categoryRepository.CreateAsync(category, cancellationToken);

            return category;
        }
    }
}
