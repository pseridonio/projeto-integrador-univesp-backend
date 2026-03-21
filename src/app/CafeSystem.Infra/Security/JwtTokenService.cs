using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CafeSystem.Infra.Security
{
    /// <summary>
    /// Serviço simples para geração de JWT para desenvolvimento.
    /// Em produção recomenda-se usar RS256 e armazenar chaves em cofre de segredos.
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user)
        {

            string key = _configuration["Jwt:Key"] ?? "dev_secret_key_please_change";
            string issuer = _configuration["Jwt:Issuer"] ?? "CafeSystem";
            string audience = _configuration["Jwt:Audience"] ?? "CafeSystemAudience";
            int expiresInMinutes = GetAccessTokenExpiryMinutes();

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.FullName ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public int GetAccessTokenExpiryMinutes()
        {
            return int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out int value) ? value : 60;
        }

        public int GetRefreshTokenExpiryMinutes()
        {
            return int.TryParse(_configuration["Jwt:RefreshExpiresInMinutes"], out int value) ? value : 60 * 24 * 7;
        }
    }
}
