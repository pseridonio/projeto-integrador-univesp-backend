using System;
using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por excluir usuários (exclusão lógica).
    /// </summary>
    public class DeleteUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserHandler(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(Guid targetUserId, Guid actingUserId, CancellationToken cancellationToken = default)
        {
            if (targetUserId == actingUserId)
            {
                throw new ArgumentException("Não é possível excluir a si mesmo");
            }

            User? user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            if (!user.IsActive)
            {
                // already deleted -> nothing to do
                return;
            }

            // Begin transaction: ensure user deletion and token revocation occur atomically
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                user.MarkDeleted(actingUserId);
                await _userRepository.UpdateAsync(user, cancellationToken);

                // revoke tokens
                await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
