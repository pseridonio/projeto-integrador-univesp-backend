namespace CafeSystem.Application.Validation
{
    public static class CategoryValidationHelper
    {
        public static bool IsValidDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return false;
            }

            string trimmedDescription = description.Trim();
            if (trimmedDescription.Length < 5 || trimmedDescription.Length > 50)
            {
                return false;
            }

            foreach (char character in trimmedDescription)
            {
                if (!char.IsLetterOrDigit(character))
                {
                    return false;
                }
            }

            return true;
        }

        public static string NormalizeDescription(string description)
        {
            return description.Trim();
        }
    }
}
