using System;

namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// DTO de saída para consultas de usuário.
    /// </summary>
    public class GetUserResponse
    {
        public Guid Code { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Data de nascimento no formato yyyy-MM-dd.
        /// </summary>
        public string? BirthDate { get; set; }
    }
}
