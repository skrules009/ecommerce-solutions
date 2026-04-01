using api_catalog_service.Data;
using api_catalog_service.DTOs;
using api_catalog_service.Models;
using api_catalog_service.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace api_catalog_service.Services
{
    /// <summary>
    /// Service for product operations
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly CatalogDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(CatalogDbContext context, ILogger<ProductService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            if (createProductDto == null)
                throw new ArgumentNullException(nameof(createProductDto));

            try
            {
                // Check if SKU already exists
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Sku == createProductDto.Sku && !p.IsDeleted);

                if (existingProduct != null)
                {
                    throw new InvalidOperationException($"Product with SKU '{createProductDto.Sku}' already exists");
                }

                var product = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Category = createProductDto.Category,
                    Sku = createProductDto.Sku,
                    Stock = createProductDto.Stock,
                    Status = createProductDto.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                Log.Information("✅ Product created: {ProductId} - {ProductName}", product.Id, product.Name);

                return MapToDto(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error creating product");
                throw;
            }
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (product == null)
                {
                    Log.Warning("⚠️ Product not found: {ProductId}", id);
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                return MapToDto(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error getting product by ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return products.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error getting all products");
                throw;
            }
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
                throw new ArgumentNullException(nameof(updateProductDto));

            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (product == null)
                {
                    Log.Warning("⚠️ Product not found for update: {ProductId}", id);
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateProductDto.Name))
                    product.Name = updateProductDto.Name;

                if (!string.IsNullOrWhiteSpace(updateProductDto.Description))
                    product.Description = updateProductDto.Description;

                if (updateProductDto.Price.HasValue)
                    product.Price = updateProductDto.Price.Value;

                if (!string.IsNullOrWhiteSpace(updateProductDto.Category))
                    product.Category = updateProductDto.Category;

                if (updateProductDto.Stock.HasValue)
                    product.Stock = updateProductDto.Stock.Value;

                if (!string.IsNullOrWhiteSpace(updateProductDto.Status))
                    product.Status = updateProductDto.Status;

                product.UpdatedAt = DateTime.UtcNow;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                Log.Information("✅ Product updated: {ProductId} - {ProductName}", product.Id, product.Name);

                return MapToDto(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error updating product: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (product == null)
                {
                    Log.Warning("⚠️ Product not found for deletion: {ProductId}", id);
                    throw new KeyNotFoundException($"Product with ID {id} not found");
                }

                // Soft delete
                product.IsDeleted = true;
                product.DeletedAt = DateTime.UtcNow;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                Log.Information("✅ Product deleted (soft): {ProductId}", id);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error deleting product: {ProductId}", id);
                throw;
            }
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                Sku = product.Sku,
                Stock = product.Stock,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
