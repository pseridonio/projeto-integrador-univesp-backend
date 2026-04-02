using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeSystem.Infra.Persistence
{
    /// <summary>
    /// Implementação do repositório de usuários usando Entity Framework Core.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            User? found = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return found;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            User? found = (User?)await _dbContext.Users.FindAsync(new object[] { id }, cancellationToken);
            return found;
        }

        // Ensure the new method declared in interface is implemented
        public async Task<User?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        {
            User? found = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return found;
        }

        public async Task<IReadOnlyList<User>> GetListAsync(IReadOnlyCollection<string> nameTerms, IReadOnlyCollection<string> emailTerms, CancellationToken cancellationToken = default)
        {
            IQueryable<User> query = _dbContext.Users
                .AsNoTracking()
                .Where(user => user.IsActive && user.DeletedAt == null);

            foreach (string term in NormalizeTerms(nameTerms))
            {
                query = query.Where(user => user.FullName.ToLower().Contains(term));
            }

            foreach (string term in NormalizeTerms(emailTerms))
            {
                query = query.Where(user => user.Email.ToLower().Contains(term));
            }

            List<User> users = await query
                .OrderBy(user => user.FullName)
                .ThenBy(user => user.Email)
                .ToListAsync(cancellationToken);

            return users;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static IEnumerable<string> NormalizeTerms(IEnumerable<string> terms)
        {
            return terms
                .Where(term => !string.IsNullOrWhiteSpace(term))
                .Select(term => term.Trim().ToLowerInvariant())
                .Distinct();
        }
    }
}
