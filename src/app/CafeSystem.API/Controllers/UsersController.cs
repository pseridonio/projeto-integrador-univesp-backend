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

        public UsersController(RegisterHandler registerHandler, UpdateUserHandler updateUserHandler)
        {
            _registerHandler = registerHandler;
            _updateUserHandler = updateUserHandler;
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
    }
}
