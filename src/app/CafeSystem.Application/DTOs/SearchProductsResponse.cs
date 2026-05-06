namespace CafeSystem.Application.DTOs
{
    public class SearchProductsResponse
    {
        public int Id { get; set; }

        public string Barcode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public List<SearchProductsCategoryResponse>? Categories { get; set; }
    }
}
