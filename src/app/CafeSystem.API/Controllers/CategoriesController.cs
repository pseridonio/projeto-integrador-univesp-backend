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

        public CategoriesController(CreateCategoryHandler createCategoryHandler)
        {
            _createCategoryHandler = createCategoryHandler;
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
    }
}
