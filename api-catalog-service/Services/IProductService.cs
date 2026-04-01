using api_catalog_service.DTOs;

namespace api_catalog_service.Services
{
  
        /// <summary>
        /// Interface for product service operations
        /// </summary>
        public interface IProductService
        {
            /// <summary>
            /// Create a new product
            /// </summary>
            Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);

            /// <summary>
            /// Get product by ID
            /// </summary>
            Task<ProductDto> GetProductByIdAsync(int id);

            /// <summary>
            /// Get all products
            /// </summary>
            Task<List<ProductDto>> GetAllProductsAsync();

            /// <summary>
            /// Update an existing product
            /// </summary>
            Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);

            /// <summary>
            /// Soft delete a product
            /// </summary>
            Task<bool> DeleteProductAsync(int id);
        }
    
}
