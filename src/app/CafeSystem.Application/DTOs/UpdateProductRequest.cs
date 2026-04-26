namespace CafeSystem.Application.DTOs
{
    public class UpdateProductRequest
    {
        public string Barcode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }
    }
}
