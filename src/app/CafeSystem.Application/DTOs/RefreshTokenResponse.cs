using System;

namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Resposta para renovação do token de acesso.
    /// </summary>
    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
