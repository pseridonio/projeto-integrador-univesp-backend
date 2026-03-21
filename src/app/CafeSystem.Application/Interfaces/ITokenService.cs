using System.Threading.Tasks;
using CafeSystem.Domain.Entities;

namespace CafeSystem.Application.Interfaces
{
    /// <summary>
    /// Serviço responsável por gerar e validar tokens JWT e refresh tokens.
    /// </summary>
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();

        int GetAccessTokenExpiryMinutes();

        int GetRefreshTokenExpiryMinutes();
    }
}
