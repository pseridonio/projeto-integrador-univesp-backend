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
        private readonly DeleteProductHandler _deleteProductHandler;
        private readonly SearchProductsHandler _searchProductsHandler;
        private readonly AddCategoryToProductHandler _addCategoryToProductHandler;
        private readonly RemoveCategoryFromProductHandler _removeCategoryFromProductHandler;

        public ProductsController(CreateProductHandler createProductHandler, UpdateProductHandler updateProductHandler, DeleteProductHandler deleteProductHandler, SearchProductsHandler searchProductsHandler, AddCategoryToProductHandler addCategoryToProductHandler, RemoveCategoryFromProductHandler removeCategoryFromProductHandler)
        {
            _createProductHandler = createProductHandler;
            _updateProductHandler = updateProductHandler;
            _deleteProductHandler = deleteProductHandler;
            _searchProductsHandler = searchProductsHandler;
            _addCategoryToProductHandler = addCategoryToProductHandler;
            _removeCategoryFromProductHandler = removeCategoryFromProductHandler;
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _deleteProductHandler.HandleAsync(id, cancellationToken);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound(new { message = "Produto não encontrado." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts([FromQuery] SearchProductsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                List<SearchProductsResponse> response = await _searchProductsHandler.HandleAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound(new { message = "Nenhum produto encontrado" });
            }
        }

        [HttpPost("{productId:int}/categories/{categoryId:int}")]
        public async Task<IActionResult> AddCategoryToProduct(int productId, int categoryId, CancellationToken cancellationToken)
        {
            try
            {
                bool created = await _addCategoryToProductHandler.HandleAsync(productId, categoryId, cancellationToken);
                if (!created)
                {
                    return NoContent();
                }

                return Created($"/api/products/{productId}/categories/{categoryId}", new { productId, categoryId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound(new { message = "Produto não encontrado." });
            }
        }

        [HttpDelete("{productId:int}/categories/{categoryId:int}")]
        public async Task<IActionResult> RemoveCategoryFromProduct(int productId, int categoryId, CancellationToken cancellationToken)
        {
            try
            {
                await _removeCategoryFromProductHandler.HandleAsync(productId, categoryId, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound(new { message = "Produto não encontrado." });
            }
        }
    }
}
