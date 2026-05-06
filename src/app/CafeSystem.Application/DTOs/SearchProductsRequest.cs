namespace CafeSystem.Application.DTOs
{
    public class SearchProductsRequest
    {
        public int? Id { get; set; }

        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        public string? Barcode { get; set; }

        public bool IncludeCategories { get; set; }

        public string? Sort { get; set; }
    }
}
