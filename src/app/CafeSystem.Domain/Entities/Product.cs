namespace CafeSystem.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Barcode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
