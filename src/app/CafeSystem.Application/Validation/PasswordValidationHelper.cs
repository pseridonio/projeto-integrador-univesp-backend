namespace CafeSystem.Application.Validation
{
    public static class PasswordValidationHelper
    {
        public const int MinLength = 5;
        public const int MaxLength = 20;

        public static bool IsProvided(string? password)
        {
            return !string.IsNullOrWhiteSpace(password);
        }

        public static bool HasMinimumLength(string password)
        {
            return password.Trim().Length >= MinLength;
        }

        public static bool HasMaximumLength(string password)
        {
            return password.Trim().Length <= MaxLength;
        }

        public static void Validate(string password)
        {
            if (!IsProvided(password))
            {
                throw new ArgumentException("O campo senha é obrigatório");
            }

            if (!HasMinimumLength(password))
            {
                throw new ArgumentException("Senha deve conter 5 ou mais caracteres");
            }

            if (!HasMaximumLength(password))
            {
                throw new ArgumentException("Senha deve conter no máximo 20 caracteres");
            }
        }
    }
}
