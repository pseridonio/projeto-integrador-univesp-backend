using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeSystem.Infra.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");

            builder.HasKey(x => x.Id).HasName("pk_refresh_tokens_id");

            builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
            builder.Property(x => x.Token).HasColumnName("token").HasColumnType("text").IsRequired();
            builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("uuid").IsRequired();
            builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.RevokedAt).HasColumnName("revoked_at").HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.ReplacedBy).HasColumnName("replaced_by").HasColumnType("text").IsRequired(false);
        }
    }
}
