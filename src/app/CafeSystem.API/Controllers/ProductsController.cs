using CafeSystem.Application.DTOs;
using CafeSystem.Application.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace CafeSystem.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly CreateProductHandler _createProductHandler;

        public ProductsController(CreateProductHandler createProductHandler)
        {
            _createProductHandler = createProductHandler;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            try
            {
                CafeSystem.Domain.Entities.Product product = await _createProductHandler.HandleAsync(request, cancellationToken);

                CreateProductResponse response = new CreateProductResponse
                {
                    Id = product.Id
                };

                return Created($"/api/products/{product.Id}", response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
