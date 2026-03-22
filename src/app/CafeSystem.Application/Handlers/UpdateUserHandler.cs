using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por atualizar dados de usuário.
    /// </summary>
    public class UpdateUserHandler
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> HandleAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            ValidateFullName(request.FullName);
            DateOnly? birthDate = ValidateBirthDate(request.BirthDate);

            User? user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("Usuário não encontrado.");
            }

            user.FullName = request.FullName.Trim();
            user.BirthDate = birthDate;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);

            return user;
        }

        private static void ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("O campo nome é obrigatório");
            }

            if (fullName.Trim().Length < 5)
            {
                throw new ArgumentException("O campo nome deve conter 5 ou mais caracteres");
            }

            if (fullName.Trim().Length > 250)
            {
                throw new ArgumentException("O campo nome deve conter no máximo 250 caracteres.");
            }
        }

        private static DateOnly? ValidateBirthDate(string? birthDate)
        {
            if (string.IsNullOrWhiteSpace(birthDate))
            {
                return null;
            }

            bool isParsed = DateOnly.TryParseExact(
                birthDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly parsedBirthDate);

            if (!isParsed || parsedBirthDate >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            {
                throw new ArgumentException("Data de nascimento inválida.");
            }

            return parsedBirthDate;
        }
    }
}
