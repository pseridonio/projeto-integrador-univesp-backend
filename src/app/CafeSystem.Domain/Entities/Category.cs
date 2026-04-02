namespace CafeSystem.Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma categoria de produto.
    /// </summary>
    public class Category
    {
        public int Code { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? DeletedAt { get; set; }
    }
}
