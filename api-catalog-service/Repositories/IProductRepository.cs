using api_catalog_service.Models;

namespace api_catalog_service.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> GetProductBySkuAsync(string sku);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        Task<bool> SkuExistsAsync(string sku);
        Task<int> GetStockAsync(int productId);
        Task UpdateStockAsync(int productId, int newQuantity);
    }
}
