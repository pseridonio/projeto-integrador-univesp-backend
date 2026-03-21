using CafeSystem.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CafeSystem.Infra.Security
{
    /// <summary>
    /// Adaptador que usa PasswordHasher do ASP.NET Core Identity.
    /// </summary>
    public class PasswordHasherAdapter : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        public bool Verify(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
