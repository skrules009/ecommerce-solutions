using api_search_service.DTOs;
using api_search_service.Models;

namespace api_search_service.Services
{
    public interface IElasticsearchService
    {
        // Index operations
        Task<bool> IndexDocumentAsync(SearchDocument document);
        Task<bool> IndexDocumentsAsync(List<SearchDocument> documents);
        Task<bool> UpdateDocumentAsync(SearchDocument document);
        Task<bool> DeleteDocumentAsync(string documentType, int documentId);
        Task<bool> DeleteByQueryAsync(string query);

        // Search operations
        Task<SearchResponseDto> SearchAsync(SearchRequestDto searchRequest);
        Task<SearchResponseDto> SearchByTypeAsync(string documentType, string query, int pageNumber, int pageSize);
        Task<SearchResponseDto> AdvancedSearchAsync(SearchRequestDto searchRequest);

        // Index management
        Task<bool> CreateIndexAsync(string indexName);
        Task<bool> DeleteIndexAsync(string indexName);
        Task<bool> IndexExistsAsync(string indexName);
    }
}
