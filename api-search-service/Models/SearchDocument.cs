namespace api_search_service.Models
{
    public class SearchDocument
    {
        public int Id { get; set; }

        public string DocumentType { get; set; } // Product, Order, Customer

        public int DocumentId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string Content { get; set; } // Full text search content

        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
