namespace CafeSystem.Application.DTOs
{
    public class CreateProductRequest
    {
        public string Barcode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public List<int> Categories { get; set; } = new List<int>();
    }
}
