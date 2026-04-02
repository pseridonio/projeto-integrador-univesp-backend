namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Resposta com item de categoria encontrado na busca.
    /// </summary>
    public class SearchCategoriesResponse
    {
        /// <summary>
        /// Código identificador da categoria.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Descrição da categoria.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
