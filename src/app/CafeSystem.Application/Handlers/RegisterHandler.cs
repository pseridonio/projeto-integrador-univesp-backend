using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Application.Validation;
using CafeSystem.Domain.Entities;
using System.Globalization;
using System.Net.Mail;

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

        public async Task<User> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            ValidateFullName(request.FullName);
            ValidateEmail(request.Email);
            PasswordValidationHelper.Validate(request.Password);
            DateOnly? birthDate = ValidateBirthDate(request.BirthDate);

            CafeSystem.Domain.Entities.User? exists = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (exists != null)
            {
                throw new InvalidOperationException("E-mail já cadastro");
            }

            PasswordHashResult passwordHashResult = _passwordHasher.Hash(request.Password);

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                BirthDate = birthDate,
                PasswordHash = passwordHashResult.Hash,
                PasswordSalt = passwordHashResult.Salt
            };

            await _userRepository.CreateAsync(user, cancellationToken);

            return user;
        }

        public async Task<User> HandleAsync(string email, string password, string fullName, CancellationToken cancellationToken = default)
        {
            CreateUserRequest request = new CreateUserRequest
            {
                Email = email,
                Password = password,
                FullName = fullName
            };

            return await HandleAsync(request, cancellationToken);
        }

        private static void ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Campo nome é obrigatório");
            }

            if (fullName.Trim().Length < 5)
            {
                throw new ArgumentException("Nome deve conter 5 ou mais caracteres");
            }

            if (fullName.Trim().Length > 250)
            {
                throw new ArgumentException("Nome deve conter no máximo 250 caracteres");
            }
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Campo e-mail é obrigatório");
            }

            try
            {
                MailAddress mailAddress = new MailAddress(email);
                if (!string.Equals(mailAddress.Address, email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Campo e-mail está em um formato inválido");
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException("Campo e-mail está em um formato inválido");
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
                throw new ArgumentException("Data de nascimento inválida");
            }

            return parsedBirthDate;
        }
    }
}
