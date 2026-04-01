using api_catalog_service.Data;
using api_catalog_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_catalog_service.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;

        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> GetProductBySkuAsync(string sku)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Sku.ToLower() == sku.ToLower());
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category.ToLower() == category.ToLower())
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> SkuExistsAsync(string sku)
        {
            return await _context.Products
                .AnyAsync(p => p.Sku.ToLower() == sku.ToLower());
        }

        public async Task<int> GetStockAsync(int productId)
        {
            var product = await GetProductByIdAsync(productId);
            return product?.StockQuantity ?? 0;
        }

        public async Task UpdateStockAsync(int productId, int newQuantity)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.StockQuantity = newQuantity;
                product.UpdatedAt = DateTime.UtcNow;
                await UpdateProductAsync(product);
            }
        }
    }
}
