using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using CafeSystem.Application.Interfaces;
using CafeSystem.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System;

namespace CafeSystem.API.Filters
{
    /// <summary>
    /// Authorization filter that enforces presence of a valid bearer token on non-anonymous endpoints.
    /// It accepts either a stored refresh token (persisted) or a valid JWT access token.
    /// </summary>
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public AuthorizationFilter(IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // NOTE (production):
            // - It's recommended to validate JWT issuer/audience and signing key strictly (see Program.cs comments).
            // - Prefer asymmetric signing (RS256) and store keys in a secure secret store.
            // - Refresh tokens stored in the database should NOT generally be accepted as bearer tokens for normal API access.
            //   The filter currently accepts a persisted refresh token for convenience; consider removing that path
            //   and only allowing refresh tokens to be used at the dedicated refresh endpoint.

            // Skip authorization for endpoints marked with [AllowAnonymous]
            Endpoint? endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
                return;

            IHeaderDictionary headers = context.HttpContext.Request.Headers;
            if (!headers.ContainsKey("Authorization"))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            string? auth = headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            string token = auth.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // First, try to find the token as a stored refresh token
            RefreshToken? stored = await _refreshTokenRepository.GetByTokenAsync(token);
            if (stored != null && stored.IsActive)
            {
                // attach a simple principal with user id so controllers can read it if needed
                Claim[] claims = new[] { new Claim(ClaimTypes.NameIdentifier, stored.UserId.ToString()) };
                context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "RefreshToken"));
                return;
            }

            // Otherwise, try to validate as JWT access token
            try
            {
                string key = _configuration["Jwt:Key"] ?? "dev_secret_key_please_change";
                string? issuer = _configuration["Jwt:Issuer"];
                string? audience = _configuration["Jwt:Audience"];

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken? validatedToken);
                // attach the principal so downstream can use it
                context.HttpContext.User = principal;
                return;
            }
            catch
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}
