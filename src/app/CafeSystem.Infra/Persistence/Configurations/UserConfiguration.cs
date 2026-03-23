using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace CafeSystem.Infra.Persistence.Configurations
{
    /// <summary>
    /// Configuração de mapeamento da entidade User para PostgreSQL usando Fluent API.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id).HasName("pk_users_id");

            builder.Property(u => u.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .HasColumnType("varchar(255)")
                .IsRequired();

            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(u => u.PasswordSalt)
                .HasColumnName("password_salt")
                .HasColumnType("varchar(128)")
                .IsRequired();

            builder.Property(u => u.FullName)
                .HasColumnName("full_name")
                .HasColumnType("varchar(250)")
                .IsRequired();

            builder.Property(u => u.BirthDate)
                .HasColumnName("birth_date")
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(u => u.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            // Map roles as JSONB column
            var converter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            builder.Property(u => u.Roles)
                .HasColumnName("roles")
                .HasColumnType("jsonb")
                .HasConversion(converter);

            builder.HasIndex(u => u.Email)
                .HasDatabaseName("ux_users_email")
                .IsUnique();
        }
    }
}
