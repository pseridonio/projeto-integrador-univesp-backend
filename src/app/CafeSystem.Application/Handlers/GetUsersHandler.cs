using CafeSystem.Application.DTOs;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using System.Globalization;

namespace CafeSystem.Application.Handlers
{
    /// <summary>
    /// Handler responsável por consultar lista de usuários com filtros opcionais.
    /// </summary>
    public class GetUsersHandler
    {
        private readonly IUserRepository _userRepository;

        public GetUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IReadOnlyList<GetUserResponse>> HandleAsync(IEnumerable<string>? nameTerms, IEnumerable<string>? emailTerms, CancellationToken cancellationToken = default)
        {
            List<string> normalizedNameTerms = NormalizeTerms(nameTerms);
            List<string> normalizedEmailTerms = NormalizeTerms(emailTerms);

            IReadOnlyList<User> users = await _userRepository.GetListAsync(normalizedNameTerms, normalizedEmailTerms, cancellationToken);
            if (users.Count == 0)
            {
                throw new InvalidOperationException("NOT_FOUND");
            }

            List<GetUserResponse> response = users
                .Select(user => new GetUserResponse
                {
                    Code = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    BirthDate = user.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                })
                .ToList();

            return response;
        }

        private static List<string> NormalizeTerms(IEnumerable<string>? terms)
        {
            if (terms == null)
            {
                return new List<string>();
            }

            List<string> normalizedTerms = new List<string>();
            foreach (string term in terms)
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    continue;
                }

                string[] parts = term.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string part in parts)
                {
                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        normalizedTerms.Add(part.Trim());
                    }
                }
            }

            return normalizedTerms;
        }
    }
}
