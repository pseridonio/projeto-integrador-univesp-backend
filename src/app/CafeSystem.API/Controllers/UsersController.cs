using System;
using System.Threading;
using System.Threading.Tasks;
using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSystem.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly RegisterHandler _registerHandler;
        private readonly UpdateUserHandler _updateUserHandler;
        private readonly DeleteUserHandler _deleteUserHandler;

        public UsersController(RegisterHandler registerHandler, UpdateUserHandler updateUserHandler, DeleteUserHandler deleteUserHandler)
        {
            _registerHandler = registerHandler;
            _updateUserHandler = updateUserHandler;
            _deleteUserHandler = deleteUserHandler;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                CafeSystem.Domain.Entities.User user = await _registerHandler.HandleAsync(request, cancellationToken);
                return Created($"/api/users/{user.Id}", new { code = user.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                CafeSystem.Domain.Entities.User user = await _updateUserHandler.HandleAsync(id, request, cancellationToken);
                return Ok(new { code = user.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                // obtain acting user id from principal
                string? nameId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(nameId) || !Guid.TryParse(nameId, out Guid actingUserId))
                {
                    return Unauthorized();
                }

                await _deleteUserHandler.HandleAsync(id, actingUserId, cancellationToken);

                // If target not found, handler throws InvalidOperationException with "NOT_FOUND"
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }
        }
    }
}
