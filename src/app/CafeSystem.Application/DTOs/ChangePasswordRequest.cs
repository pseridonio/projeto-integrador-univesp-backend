namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Requisição para troca de senha do usuário autenticado.
    /// </summary>
    public class ChangePasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}
