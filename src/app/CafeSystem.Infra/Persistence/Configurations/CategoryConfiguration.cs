using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeSystem.Infra.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("categories");

            builder.HasKey(x => x.Code).HasName("pk_categories_code");

            builder.Property(x => x.Code)
                .HasColumnName("code")
                .HasColumnType("integer")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
        }
    }
}
