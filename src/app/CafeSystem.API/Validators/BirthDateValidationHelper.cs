using System.Globalization;

namespace CafeSystem.API.Validators
{
    internal static class BirthDateValidationHelper
    {
        public static bool BeValidBirthDate(string? birthDate)
        {
            if (string.IsNullOrWhiteSpace(birthDate))
            {
                return true;
            }

            bool isParsed = DateOnly.TryParseExact(
                birthDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly parsedBirthDate);

            if (!isParsed)
            {
                return false;
            }

            return parsedBirthDate < DateOnly.FromDateTime(DateTime.UtcNow.Date);
        }
    }
}
