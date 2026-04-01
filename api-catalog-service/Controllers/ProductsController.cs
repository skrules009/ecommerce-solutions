using api_catalog_service.DTOs;
using api_catalog_service.Events;
using api_catalog_service.Publishers;
using api_catalog_service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api_catalog_service.Controllers
{
    /// <summary>
    /// Product API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IEventPublisher eventPublisher,
            ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>List of all products</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductDto>>> GetAllProducts()
        {
            try
            {
                Log.Information("GET: Retrieving all products");
                var products = await _productService.GetAllProductsAsync();
               
                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error retrieving all products");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            if (id <= 0)
            {
                Log.Warning("⚠️ Invalid product ID: {ProductId}", id);
                return BadRequest(new { message = "Product ID must be greater than 0" });
            }

            try
            {
                Log.Information("GET: Retrieving product {ProductId}", id);
                var product = await _productService.GetProductByIdAsync(id);
                Log.Information("✅ Product retrieved: {ProductId}", id);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("⚠️ Product not found: {ProductId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error retrieving product {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="createProductDto">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("⚠️ Invalid model state for product creation");
                return BadRequest(ModelState);
            }

            Log.Information("POST: New product creation requested for SKU {Sku}", createProductDto.Sku);

            try
            {
                // 1. Create product in database
                var product = await _productService.CreateProductAsync(createProductDto);
                Log.Information("✅ Product created: {ProductId} - {ProductName}", product.Id, product.Name);

                // 2. Publish event for SearchService and other subscribers
                try
                {
                    var productAddedEvent = new ProductAddedEvent
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Category = product.Category,
                        Sku = product.Sku,
                        Status = product.Status,
                        CreatedAt = product.CreatedAt
                    };

                    await _eventPublisher.PublishProductAddedEventAsync(productAddedEvent);  // ✅ Synchronous call
                    Log.Information("✅ ProductAddedEvent published: {ProductId}", product.Id);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "⚠️ Event publishing failed for Product {ProductId}, but product was created", product.Id);
                    // Don't throw - product was created successfully, event publishing failure shouldn't break response
                }

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning("⚠️ Validation error: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Unexpected error creating product");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateProductDto">Updated product data</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (id <= 0)
            {
                Log.Warning("⚠️ Invalid product ID: {ProductId}", id);
                return BadRequest(new { message = "Product ID must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                Log.Warning("⚠️ Invalid model state for product update");
                return BadRequest(ModelState);
            }

            Log.Information("PUT: Product update requested for ID {ProductId}", id);

            try
            {
                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                Log.Information("✅ Product updated: {ProductId}", id);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("⚠️ Product not found: {ProductId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error updating product {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
            {
                Log.Warning("⚠️ Invalid product ID: {ProductId}", id);
                return BadRequest(new { message = "Product ID must be greater than 0" });
            }

            Log.Information("DELETE: Product deletion requested for ID {ProductId}", id);

            try
            {
                await _productService.DeleteProductAsync(id);
                Log.Information("✅ Product deleted: {ProductId}", id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning("⚠️ Product not found: {ProductId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error deleting product {ProductId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
