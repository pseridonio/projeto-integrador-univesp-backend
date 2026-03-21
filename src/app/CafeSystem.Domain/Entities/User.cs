using System;
using System.Collections.Generic;

namespace CafeSystem.Domain.Entities
{
    /// <summary>
    /// Entidade de domínio que representa um usuário do sistema.
    /// Contém apenas propriedades essenciais ao domínio.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<string> Roles { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Marca o usuário como inativo (exclusão lógica).
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
