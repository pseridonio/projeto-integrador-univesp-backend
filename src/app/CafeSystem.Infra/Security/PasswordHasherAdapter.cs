using CafeSystem.Application.Interfaces;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace CafeSystem.Infra.Security
{
    /// <summary>
    /// Adaptador que usa Argon2id com salt aleatório por usuário e pepper de configuração.
    /// </summary>
    public class PasswordHasherAdapter : IPasswordHasher
    {
        private readonly string _pepper;

        public PasswordHasherAdapter(IConfiguration configuration)
        {
            _pepper = configuration["PasswordHashing:Pepper"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_pepper))
            {
                throw new InvalidOperationException("PasswordHashing:Pepper não configurado");
            }
        }

        public PasswordHashResult Hash(string password)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
            string salt = Convert.ToBase64String(saltBytes);
            string hash = ComputeHash(password, saltBytes);

            return new PasswordHashResult
            {
                Hash = hash,
                Salt = salt
            };
        }

        public bool Verify(string hashedPassword, string salt, string providedPassword)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            string providedHash = ComputeHash(providedPassword, saltBytes);

            byte[] expectedBytes = Convert.FromBase64String(hashedPassword);
            byte[] providedBytes = Convert.FromBase64String(providedHash);

            return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }

        private string ComputeHash(string password, byte[] saltBytes)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes($"{password}{_pepper}");

            Argon2id argon2id = new Argon2id(passwordBytes)
            {
                Salt = saltBytes,
                DegreeOfParallelism = 4,
                Iterations = 4,
                MemorySize = 65536
            };

            byte[] hashBytes = argon2id.GetBytes(32);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
