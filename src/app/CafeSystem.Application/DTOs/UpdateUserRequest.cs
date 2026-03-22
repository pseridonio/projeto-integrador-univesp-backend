namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Requisição para atualização de usuário.
    /// </summary>
    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string? BirthDate { get; set; }
    }
}
