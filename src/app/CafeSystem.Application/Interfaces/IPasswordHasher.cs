namespace CafeSystem.Application.Interfaces
{
    /// <summary>
    /// Abstração simples para hashing de senhas.
    /// Implementação pode usar Microsoft.AspNetCore.Identity.PasswordHasher ou biblioteca externa.
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string password);

        bool Verify(string hashedPassword, string providedPassword);
    }
}
