using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace CafeSystem.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly CreateCategoryHandler _createCategoryHandler;
        private readonly GetCategoryByCodeHandler _getCategoryByCodeHandler;
        private readonly UpdateCategoryHandler _updateCategoryHandler;

        public CategoriesController(CreateCategoryHandler createCategoryHandler, GetCategoryByCodeHandler getCategoryByCodeHandler, UpdateCategoryHandler updateCategoryHandler)
        {
            _createCategoryHandler = createCategoryHandler;
            _getCategoryByCodeHandler = getCategoryByCodeHandler;
            _updateCategoryHandler = updateCategoryHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                CafeSystem.Domain.Entities.Category category = await _createCategoryHandler.HandleAsync(request, cancellationToken);
                CreateCategoryResponse response = new CreateCategoryResponse
                {
                    Code = category.Code
                };

                return Created($"/api/categories/{category.Code}", response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{code:int}")]
        public async Task<IActionResult> GetCategory(int code, CancellationToken cancellationToken)
        {
            try
            {
                GetCategoryResponse response = await _getCategoryByCodeHandler.HandleAsync(code, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound(new { message = "Categoria não encontrada." });
            }
        }

        [HttpPut("{code:int}")]
        public async Task<IActionResult> UpdateCategory(int code, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _updateCategoryHandler.HandleAsync(code, request, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound(new { message = "Categoria não encontrada." });
            }
        }
    }
}
