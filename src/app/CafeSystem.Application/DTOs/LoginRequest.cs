using System;

namespace CafeSystem.Application.DTOs
{
    /// <summary>
    /// Requisição para operação de login.
    /// Contém credenciais do usuário.
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
