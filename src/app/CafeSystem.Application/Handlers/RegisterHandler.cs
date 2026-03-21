using System;
using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using CafeSystem.Application.Interfaces;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler para registrar novos usuários.
    /// </summary>
    public class RegisterHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> HandleAsync(string email, string password, string fullName, CancellationToken cancellationToken = default)
        {
            CafeSystem.Domain.Entities.User? exists = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (exists != null)
                throw new InvalidOperationException("Email already registered");

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullName
            };

            user.PasswordHash = _passwordHasher.Hash(password);

            await _userRepository.CreateAsync(user, cancellationToken);

            return user;
        }
    }
}
