using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeSystem.Infra.Persistence
{
    /// <summary>
    /// DbContext para persistência do aplicativo.
    /// Utiliza mapeamentos básicos e tabelas necessárias.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations from separate configuration classes
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); // Updated comment
        }
    }
}
