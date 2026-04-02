using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler para busca de categorias por descrição.
    /// </summary>
    public class SearchCategoriesHandler
    {
        private readonly ICategoryRepository _categoryRepository;

        public SearchCategoriesHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Busca categorias por descrição com suporte a múltiplos termos.
        /// </summary>
        /// <param name="request">Requisição com termo de busca.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de categorias encontradas.</returns>
        /// <exception cref="InvalidOperationException">Lançada quando nenhuma categoria é encontrada.</exception>
        public async Task<List<SearchCategoriesResponse>> HandleAsync(SearchCategoriesRequest request, CancellationToken cancellationToken = default)
        {
            List<Category> categories = await _categoryRepository.SearchByDescriptionAsync(request.Description, cancellationToken);

            if (categories.Count == 0)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            return categories
                .Select(x => new SearchCategoriesResponse
                {
                    Code = x.Code,
                    Description = x.Description
                })
                .ToList();
        }
    }
}
