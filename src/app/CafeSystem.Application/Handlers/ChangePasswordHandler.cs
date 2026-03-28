using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Application.Validation;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por trocar a senha do usuário autenticado.
    /// </summary>
    public class ChangePasswordHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ChangePasswordHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> HandleAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            PasswordValidationHelper.Validate(request.Password);

            User? user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || !user.IsActive)
            {
                throw new InvalidOperationException("Usuário não encontrado.");
            }

            PasswordHashResult passwordHashResult = _passwordHasher.Hash(request.Password);
            user.PasswordHash = passwordHashResult.Hash;
            user.PasswordSalt = passwordHashResult.Salt;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);

            return user;
        }
    }
}
