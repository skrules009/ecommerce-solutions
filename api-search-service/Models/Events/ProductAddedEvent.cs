namespace api_search_service.Models.Events
{
    /// <summary>
    /// Event raised when a product is added in the Catalog Service
    /// </summary>
    public class ProductAddedEvent
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Sku { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}