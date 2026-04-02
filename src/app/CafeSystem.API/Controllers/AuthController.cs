using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSystem.API.Controllers
{
    /// <summary>
    /// Controller responsável pelos endpoints de autenticação.
    /// Rotas públicas para login/registro/refresh.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LoginHandler _loginHandler;
        private readonly RefreshTokenHandler _refreshTokenHandler;
        private readonly RegisterHandler _registerHandler;

        public AuthController(LoginHandler loginHandler, RefreshTokenHandler refreshTokenHandler, RegisterHandler registerHandler)
        {
            _loginHandler = loginHandler;
            _refreshTokenHandler = refreshTokenHandler;
            _registerHandler = registerHandler;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            LoginResponse? result = await _loginHandler.HandleAsync(request);
            if (result == null)
                return Unauthorized(new { message = "E-mail ou senha inválidos" });

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            RefreshTokenResponse? result = await _refreshTokenHandler.HandleAsync(request);
            if (result == null)
                return Unauthorized(new { message = "E-mail ou senha inválidos" });

            return Ok(result);
        }
    }
}
