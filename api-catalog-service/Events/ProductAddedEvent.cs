namespace api_catalog_service.Events
{
    /// <summary>
    /// Event published when a new product is added to the catalog
    /// </summary>
    public class ProductAddedEvent
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Sku { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Combined content for full-text search in Elasticsearch
        /// </summary>
        public string Content => $"{Name} {Description} {Category} {Sku}";
    }
}
