namespace CafeSystem.Domain.Entities
{
    /// <summary>
    /// Entidade que representa um refresh token persistido no banco.
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedBy { get; set; }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow <= ExpiresAt;
    }
}
