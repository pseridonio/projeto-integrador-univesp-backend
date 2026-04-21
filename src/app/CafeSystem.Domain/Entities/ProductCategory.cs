namespace CafeSystem.Domain.Entities
{
    public class ProductCategory
    {
        public int ProductId { get; set; }

        public int CategoryCode { get; set; }

        public Product Product { get; set; } = null!;

        public Category Category { get; set; } = null!;
    }
}
