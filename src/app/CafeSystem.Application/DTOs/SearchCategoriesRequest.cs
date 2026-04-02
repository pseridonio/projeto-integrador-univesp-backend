namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Requisição para busca de categorias por descrição.
    /// </summary>
    public class SearchCategoriesRequest
    {
        /// <summary>
        /// Descrição ou termo de busca para pesquisar categorias.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
