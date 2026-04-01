namespace api_search_service.DTOs
{
    public class SearchResponseDto
    {
        public List<SearchResultDto> Results { get; set; } = new();

        public long TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasNextPage => PageNumber < TotalPages;

        public bool HasPreviousPage => PageNumber > 1;

        public double ExecutionTimeMs { get; set; }
    }
}
