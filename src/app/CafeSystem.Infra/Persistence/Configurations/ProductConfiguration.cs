using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeSystem.Infra.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.HasKey(x => x.Id).HasName("pk_products_id");

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("integer")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Barcode)
                .HasColumnName("barcode")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.HasIndex(x => x.Barcode)
                .HasDatabaseName("ux_products_barcode_active")
                .IsUnique()
                .HasFilter("\"is_deleted\" = false");

            builder.HasMany(x => x.ProductCategories)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .HasConstraintName("fk_product_categories_product_id")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
