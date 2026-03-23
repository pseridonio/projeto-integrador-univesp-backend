namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Resposta de login contendo tokens e informações do usuário.
    /// </summary>
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
