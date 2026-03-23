using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CafeSystem.Infra.Persistence
{
    /// <summary>
    /// Responsável por executar a rotina de seed durante a primeira execução do sistema.
    /// </summary>
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private const string DefaultAdminEmail = "admin@admin.com";
        private const string DefaultAdminPassword = "ABC123*";
        private const string DefaultAdminName = "Admin";

        private static readonly DateOnly DefaultAdminBirthDate = new DateOnly(2000, 1, 1);

        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(AppDbContext dbContext, IPasswordHasher passwordHasher, ILogger<DatabaseInitializer> logger)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
        {
            bool hasUsers = await _dbContext.Users.AnyAsync(cancellationToken);
            if (hasUsers)
            {
                _logger.LogInformation("Tabela de usuários já possui registros. Seed ignorado.");
                return;
            }

            PasswordHashResult passwordHash = _passwordHasher.Hash(DefaultAdminPassword);

            User admin = new User
            {
                Id = Guid.NewGuid(),
                Email = DefaultAdminEmail,
                FullName = DefaultAdminName,
                BirthDate = DefaultAdminBirthDate,
                PasswordHash = passwordHash.Hash,
                PasswordSalt = passwordHash.Salt,
                IsActive = true,
                Roles = new List<string> { "Admin" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Users.AddAsync(admin, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuário administrador padrão criado com o e-mail {Email}.", DefaultAdminEmail);
        }
    }
}
