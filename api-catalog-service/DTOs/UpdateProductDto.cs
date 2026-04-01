using System.ComponentModel.DataAnnotations;

namespace api_catalog_service.DTOs
{
    /// <summary>
    /// DTO for updating an existing product
    /// </summary>
    public class UpdateProductDto
    {
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 255 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal? Price { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 100 characters")]
        public string Category { get; set; }

        [Range(0, 999999, ErrorMessage = "Stock must be between 0 and 999999")]
        public int? Stock { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }
}
