using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using System.Globalization;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por buscar usuários por código.
    /// </summary>
    public class GetUserByIdHandler
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<GetUserResponse> HandleAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            User? user = await _userRepository.GetByIdNoTrackingAsync(userId, cancellationToken);
            if (user == null || !user.IsActive || user.DeletedAt.HasValue)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            return new GetUserResponse
            {
                Code = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                BirthDate = user.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }
    }
}
