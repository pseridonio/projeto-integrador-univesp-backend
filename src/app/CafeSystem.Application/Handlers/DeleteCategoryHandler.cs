using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    public class DeleteCategoryHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public DeleteCategoryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task HandleAsync(int code, CancellationToken cancellationToken = default)
        {
            Category? category = await _categoryRepository.GetByCodeNoTrackingAsync(code, cancellationToken);
            if (category == null)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            if (!category.IsActive || category.DeletedAt.HasValue)
            {
                return;
            }

            category.IsActive = false;
            category.DeletedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category, cancellationToken);
        }
    }
}
