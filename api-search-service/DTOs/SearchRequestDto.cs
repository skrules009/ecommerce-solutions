namespace api_search_service.DTOs
{
    public class SearchRequestDto
    {
        public string Query { get; set; }

        public string DocumentType { get; set; } // Optional filter

        public int PageNumber { get; set; } = 1;


        public int PageSize { get; set; } = 10;


        public string SortBy { get; set; } = "relevance"; // relevance, date, price


        public string SortOrder { get; set; } = "desc"; // asc, desc


        public Dictionary<string, object> Filters { get; set; } = new();
    }
}
