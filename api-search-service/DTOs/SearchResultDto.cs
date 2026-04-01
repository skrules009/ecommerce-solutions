namespace api_search_service.DTOs
{
    public class SearchResultDto
    {
        public int Id { get; set; }

        public string DocumentType { get; set; }

        public int DocumentId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public double Score { get; set; } // Elasticsearch relevance score

        public Dictionary<string, object> Metadata { get; set; }
    }
}
