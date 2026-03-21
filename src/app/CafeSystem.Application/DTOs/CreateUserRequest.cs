namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Requisição para criação de usuário.
    /// </summary>
    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? BirthDate { get; set; }
    }
}
