using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeSystem.Infra.Persistence.Configurations
{
    public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("product_categories");

            builder.HasKey(x => new { x.ProductId, x.CategoryCode })
                .HasName("pk_product_categories");

            builder.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("integer")
                .IsRequired();

            builder.Property(x => x.CategoryCode)
                .HasColumnName("category_code")
                .HasColumnType("integer")
                .IsRequired();

            builder.HasOne(x => x.Category)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.CategoryCode)
                .HasConstraintName("fk_product_categories_category_code")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
