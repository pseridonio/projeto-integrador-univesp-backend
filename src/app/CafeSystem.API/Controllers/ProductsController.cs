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
        private readonly UpdateProductHandler _updateProductHandler;

        public ProductsController(CreateProductHandler createProductHandler, UpdateProductHandler updateProductHandler)
        {
            _createProductHandler = createProductHandler;
            _updateProductHandler = updateProductHandler;
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _updateProductHandler.HandleAsync(id, request, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound();
            }
        }
    }
}
