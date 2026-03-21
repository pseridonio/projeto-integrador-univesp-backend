namespace CafeSystem.Application.Interfaces
{
    /// <summary>
    /// Abstração simples para hashing de senhas.
    /// Implementação pode usar Microsoft.AspNetCore.Identity.PasswordHasher ou biblioteca externa.
    /// </summary>
    public interface IPasswordHasher
    {
        PasswordHashResult Hash(string password);

        bool Verify(string hashedPassword, string salt, string providedPassword);
    }

    public sealed class PasswordHashResult
    {
        public string Hash { get; set; } = string.Empty;

        public string Salt { get; set; } = string.Empty;
    }
}
