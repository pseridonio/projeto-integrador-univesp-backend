namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Requisição para renovação do token de acesso.
    /// </summary>
    public class RefreshTokenRequest
    {
        public string Token { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}
