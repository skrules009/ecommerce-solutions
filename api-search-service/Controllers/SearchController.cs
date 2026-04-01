using api_search_service.DTOs;
using api_search_service.Models;
using api_search_service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace api_search_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticsearchService _searchService;

        public SearchController(IElasticsearchService searchService)
        {
            _searchService = searchService;
        }

        // ==================== INDEXING ====================
        /// <summary>
        /// Index a single document
        /// </summary>
        [HttpPost("index")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IndexDocument([FromBody] SearchDocument document)
        {
            Log.Information("=== POST /api/search/index received ===");
            Log.Information("Document: {@Document}", document);

            if (document == null)
            {
                Log.Error("Document is null!");
                return BadRequest(new { message = "Document cannot be null" });
            }

            if (string.IsNullOrEmpty(document.DocumentType))
            {
                Log.Error("DocumentType is null or empty!");
                return BadRequest(new { message = "DocumentType is required" });
            }

            try
            {
                Log.Information("Calling _searchService.IndexDocumentAsync...");
                var result = await _searchService.IndexDocumentAsync(document);

                Log.Information("IndexDocumentAsync returned: {Result}", result);

                if (!result)
                {
                    Log.Error("IndexDocumentAsync returned false");
                    return BadRequest(new { message = "Failed to index document" });
                }

                Log.Information("✅ Document indexed successfully");
                return Ok(new { message = "Document indexed successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception in IndexDocument");
                Log.Error("Exception Message: {Message}", ex.Message);
                Log.Error("Exception Type: {Type}", ex.GetType().FullName);
                return BadRequest(new { message = $"Error: {ex.Message}" });
            }
        }
        /// <summary>
        /// Bulk index documents
        /// </summary>
        [HttpPost("bulk-index")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkIndexDocuments([FromBody] List<SearchDocument> documents)
        {
            Log.Information("POST: Bulk indexing {Count} documents", documents.Count);

            try
            {
                var result = await _searchService.IndexDocumentsAsync(documents);
                if (!result)
                    return BadRequest(new { message = "Failed to bulk index documents" });

                return Ok(new { message = $"Bulk indexed {documents.Count} documents" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error bulk indexing documents");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a document
        /// </summary>
        [HttpPut("index/{documentType}/{documentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDocument(
            string documentType,
            int documentId,
            [FromBody] SearchDocument document)
        {
            Log.Information("PUT: Updating document {DocumentType}_{DocumentId}", documentType, documentId);

            try
            {
                document.DocumentType = documentType;
                document.DocumentId = documentId;

                var result = await _searchService.UpdateDocumentAsync(document);
                if (!result)
                    return BadRequest(new { message = "Failed to update document" });

                return Ok(new { message = "Document updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating document");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        [HttpDelete("index/{documentType}/{documentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(string documentType, int documentId)
        {
            Log.Information("DELETE: Deleting document {DocumentType}_{DocumentId}", documentType, documentId);

            try
            {
                var result = await _searchService.DeleteDocumentAsync(documentType, documentId);
                if (!result)
                    return NotFound(new { message = "Document not found or failed to delete" });

                return Ok(new { message = "Document deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting document");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== SEARCHING ====================
        /// <summary>
        /// Simple search
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResponseDto>> Search([FromBody] SearchRequestDto searchRequest)
        {
            Log.Information("POST: Searching for {Query}", searchRequest.Query);

            try
            {
                var result = await _searchService.SearchAsync(searchRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search by document type
        /// </summary>
        [HttpGet("search/{documentType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResponseDto>> SearchByType(
            string documentType,
            [FromQuery] string query,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            Log.Information("GET: Searching {DocumentType} for {Query}", documentType, query);

            try
            {
                var result = await _searchService.SearchByTypeAsync(documentType, query, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching by type");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Advanced search with filters
        /// </summary>
        [HttpPost("search/advanced")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SearchResponseDto>> AdvancedSearch([FromBody] SearchRequestDto searchRequest)
        {
            Log.Information("POST: Advanced search for {Query}", searchRequest.Query);

            try
            {
                var result = await _searchService.AdvancedSearchAsync(searchRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error performing advanced search");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
