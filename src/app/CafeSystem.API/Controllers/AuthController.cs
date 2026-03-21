using System.Threading.Tasks;
using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            RefreshTokenResponse? result = await _refreshTokenHandler.HandleAsync(request);
            if (result == null)
                return Unauthorized(new { message = "E-mail ou senha inválidos" });

            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            // Reusing LoginRequest for simplicity: email + password. full name will be email local-part
            string fullName = request.Email.Split('@')[0];
            try
            {
                CafeSystem.Domain.Entities.User user = await _registerHandler.HandleAsync(request.Email, request.Password, fullName);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.Email, user.FullName });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
