using api_search_service.DTOs;
using api_search_service.Models;
using Nest;
using Serilog;
using System.Diagnostics;
using Elasticsearch.Net;

namespace api_search_service.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _defaultIndex = "search-documents";

        public ElasticsearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        // ==================== INDEX OPERATIONS ====================
        public async Task<bool> IndexDocumentAsync(SearchDocument document)
        {
            Log.Information("=== Starting IndexDocumentAsync ===");
            Log.Information("Document Type: {DocumentType}", document.DocumentType);
            Log.Information("Document ID: {DocumentId}", document.DocumentId);
            Log.Information("Document Title: {Title}", document.Title);
            Log.Information("Document: {@Document}", document);

            try
            {
                if (document == null)
                {
                    Log.Error("Document is null!");
                    return false;
                }

                var indexName = GetIndexName(document.DocumentType);
                Log.Information("Index name calculated: {IndexName}", indexName);

                var documentId = $"{document.DocumentType}_{document.DocumentId}";
                Log.Information("Document ID for ES: {DocumentId}", documentId);

                Log.Information("Attempting to index...");

                var response = await _elasticClient.IndexAsync(document, i => i
                    .Index(indexName)
                    .Id(documentId)
                );

                Log.Information("Index response received");
                Log.Information("Response IsValid: {IsValid}", response.IsValid);
                
                Log.Information("Response DebugInfo: {DebugInfo}", response.DebugInformation);

                if (!response.IsValid)
                {
                    Log.Error("Response is invalid!");
                    Log.Error("Server Error: {ServerError}", response.ServerError);

                    if (response.ServerError != null)
                    {
                        Log.Error("Server Error Type: {ErrorType}", response.ServerError.Error?.Type);
                        Log.Error("Server Error Reason: {ErrorReason}", response.ServerError.Error?.Reason);
                    }

                    Log.Error("Request Exception: {RequestException}", response.OriginalException);

                    if (response.OriginalException != null)
                    {
                        Log.Error(response.OriginalException, "Original Exception Details");
                    }

                    return false;
                }

                Log.Information("✅ Document indexed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("❌ EXCEPTION caught in IndexDocumentAsync");
                Log.Error(ex, "Exception Type: {ExceptionType}", ex.GetType().FullName);
                Log.Error(ex, "Exception Message: {Message}", ex.Message);
                Log.Error(ex, "Exception StackTrace: {StackTrace}", ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner Exception: {InnerMessage}", ex.InnerException.Message);
                }

                return false;
            }
        }
        public async Task<bool> IndexDocumentsAsync(List<SearchDocument> documents)
        {
            Log.Information("Bulk indexing {Count} documents", documents.Count);

            try
            {
                var bulkDescriptor = new BulkDescriptor();

                foreach (var doc in documents)
                {
                    var indexName = GetIndexName(doc.DocumentType);
                    bulkDescriptor.Index<SearchDocument>(i => i
                        .Index(indexName)
                        .Id($"{doc.DocumentType}_{doc.DocumentId}")
                        .Document(doc)
                    );
                }

                var response = await _elasticClient.BulkAsync(bulkDescriptor);

                if (!response.IsValid)
                {
                    Log.Error("Bulk indexing failed: {Error}", response.ServerError?.Error?.Reason);
                    return false;
                }

                Log.Information("Bulk indexing completed: {Count} documents", documents.Count);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error bulk indexing documents");
                return false;
            }
        }

        public async Task<bool> UpdateDocumentAsync(SearchDocument document)
        {
            Log.Information("Updating document: {DocumentType}_{DocumentId}",
                document.DocumentType, document.DocumentId);

            try
            {
                var indexName = GetIndexName(document.DocumentType);
                document.UpdatedAt = DateTime.UtcNow;

                var response = await _elasticClient.UpdateAsync<SearchDocument>(
                    $"{document.DocumentType}_{document.DocumentId}",
                    u => u
                        .Index(indexName)
                        .Doc(document)
                );

                if (!response.IsValid)
                {
                    Log.Error("Failed to update document: {Error}", response.ServerError?.Error?.Reason);
                    return false;
                }

                Log.Information("Document updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating document");
                return false;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string documentType, int documentId)
        {
            Log.Information("Deleting document: {DocumentType}_{DocumentId}", documentType, documentId);

            try
            {
                var indexName = GetIndexName(documentType);
                var response = await _elasticClient.DeleteAsync(
                    new DocumentPath<SearchDocument>($"{documentType}_{documentId}"),
                    d => d.Index(indexName)
                );

                if (!response.IsValid)
                {
                    Log.Error("Failed to delete document: {Error}", response.ServerError?.Error?.Reason);
                    return false;
                }

                Log.Information("Document deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting document");
                return false;
            }
        }

        // ==================== SEARCH OPERATIONS ====================
        public async Task<SearchResponseDto> SearchAsync(SearchRequestDto searchRequest)
        {
            Log.Information("Searching: {Query}", searchRequest.Query);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var searchDescriptor = new SearchDescriptor<SearchDocument>()
                    .Index(_defaultIndex)
                    .From((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                    .Size(searchRequest.PageSize)
                    .Query(q => q
                        .MultiMatch(m => m
                            .Query(searchRequest.Query)
                            .Fields(f => f
                                .Field(p => p.Title, 3)      // Boost title matches
                                .Field(p => p.Description, 2) // Boost description
                                .Field(p => p.Content)         // Full text search
                                .Field(p => p.Category)
                            )
                            .Fuzziness(Fuzziness.Auto)        // Fuzzy matching for typos
                        )
                    );

                // Add document type filter if specified
                if (!string.IsNullOrEmpty(searchRequest.DocumentType))
                {
                    searchDescriptor = searchDescriptor
                        .Query(q => q
                            .Bool(b => b
                                .Must(m => m
                                    .MultiMatch(mm => mm
                                        .Query(searchRequest.Query)
                                        .Fields(f => f
                                            .Field(p => p.Title, 3)
                                            .Field(p => p.Description, 2)
                                            .Field(p => p.Content)
                                            .Field(p => p.Category)
                                        )
                                        .Fuzziness(Fuzziness.Auto)
                                    )
                                )
                                .Filter(f => f
                                    .Term(t => t.DocumentType, searchRequest.DocumentType)
                                )
                            )
                        );
                }

                // Add sorting
                searchDescriptor = ApplySorting(searchDescriptor, searchRequest.SortBy, searchRequest.SortOrder);

                var response = await _elasticClient.SearchAsync<SearchDocument>(searchDescriptor);

                stopwatch.Stop();

                if (!response.IsValid)
                {
                    Log.Error("Search failed: {Error}", response.ServerError?.Error?.Reason);
                    throw new Exception($"Search failed: {response.ServerError?.Error?.Reason}");
                }

                // ✅ Use response.Hits directly - cleaner approach
                var results = response.Hits.Select(hit => new SearchResultDto
                {
                    Id = hit.Source.Id,
                    DocumentType = hit.Source.DocumentType,
                    DocumentId = hit.Source.DocumentId,
                    Title = hit.Source.Title,
                    Description = hit.Source.Description,
                    Price = hit.Source.Price,
                    Category = hit.Source.Category,
                    Status = hit.Source.Status,
                    CreatedAt = hit.Source.CreatedAt,
                    UpdatedAt = hit.Source.UpdatedAt,
                    Score = hit.Score ?? 0,  // ✅ Get score from the hit
                    Metadata = hit.Source.Metadata
                }).ToList();

                Log.Information("Search completed: {Count} results in {Ms}ms",
                    results.Count, stopwatch.ElapsedMilliseconds);

                return new SearchResponseDto
                {
                    Results = results,
                    TotalCount = response.Total,
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error performing search");
                throw;
            }
        }

        public async Task<SearchResponseDto> SearchByTypeAsync(string documentType, string query,
            int pageNumber, int pageSize)
        {
            var searchRequest = new SearchRequestDto
            {
                Query = query,
                DocumentType = documentType,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await SearchAsync(searchRequest);
        }

        public async Task<SearchResponseDto> AdvancedSearchAsync(SearchRequestDto searchRequest)
        {
            Log.Information("Advanced search: {Query}", searchRequest.Query);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var searchDescriptor = new SearchDescriptor<SearchDocument>()
                    .Index(_defaultIndex)
                    .From((searchRequest.PageNumber - 1) * searchRequest.PageSize)
                    .Size(searchRequest.PageSize)
                    .Query(q => q
                        .Bool(b =>
                        {
                            // Main search query (MUST clause)
                            b.Must(m => m
                                .MultiMatch(mm => mm
                                    .Query(searchRequest.Query)
                                    .Fields(f => f
                                        .Field(p => p.Title, 3)
                                        .Field(p => p.Description, 2)
                                        .Field(p => p.Content)
                                        .Field(p => p.Category)
                                    )
                                    .Fuzziness(Fuzziness.Auto)
                                )
                            );

                            // Document type filter (FILTER clause)
                            if (!string.IsNullOrEmpty(searchRequest.DocumentType))
                            {
                                b.Filter(f => f
                                    .Term(t => t.DocumentType, searchRequest.DocumentType)
                                );
                            }

                            // Custom filters
                            if (searchRequest.Filters != null && searchRequest.Filters.Count > 0)
                            {
                                foreach (var filter in searchRequest.Filters)
                                {
                                    switch (filter.Key.ToLower())
                                    {
                                        case "category":
                                            b.Filter(f => f
                                                .Term(t => t.Category, filter.Value.ToString())
                                            );
                                            break;

                                        case "status":
                                            b.Filter(f => f
                                                .Term(t => t.Status, filter.Value.ToString())
                                            );
                                            break;

                                        case "minprice":
                                            if (decimal.TryParse(filter.Value.ToString(), out decimal minPrice))
                                            {
                                                b.Filter(f => f
                                                    .Range(r => r
                                                        .Field(p => p.Price)
                                                        .GreaterThanOrEquals((double)minPrice)
                                                    )
                                                );
                                            }
                                            break;

                                        case "maxprice":
                                            if (decimal.TryParse(filter.Value.ToString(), out decimal maxPrice))
                                            {
                                                b.Filter(f => f
                                                    .Range(r => r
                                                        .Field(p => p.Price)
                                                        .LessThanOrEquals((double)maxPrice)
                                                    )
                                                );
                                            }
                                            break;
                                    }
                                }
                            }

                            return b;
                        })
                    );

                // Apply sorting
                searchDescriptor = ApplySorting(searchDescriptor, searchRequest.SortBy, searchRequest.SortOrder);

                var response = await _elasticClient.SearchAsync<SearchDocument>(searchDescriptor);

                stopwatch.Stop();

                if (!response.IsValid)
                {
                    Log.Error("Advanced search failed: {Error}", response.ServerError?.Error?.Reason);
                    throw new Exception($"Advanced search failed: {response.ServerError?.Error?.Reason}");
                }

                // ✅ Use response.Hits directly - consistent with SearchAsync
                var results = response.Hits.Select(hit => new SearchResultDto
                {
                    Id = hit.Source.Id,
                    DocumentType = hit.Source.DocumentType,
                    DocumentId = hit.Source.DocumentId,
                    Title = hit.Source.Title,
                    Description = hit.Source.Description,
                    Price = hit.Source.Price,
                    Category = hit.Source.Category,
                    Status = hit.Source.Status,
                    CreatedAt = hit.Source.CreatedAt,
                    UpdatedAt = hit.Source.UpdatedAt,
                    Score = hit.Score ?? 0,  // ✅ Get score from the hit
                    Metadata = hit.Source.Metadata
                }).ToList();

                Log.Information("Advanced search completed: {Count} results in {Ms}ms",
                    results.Count, stopwatch.ElapsedMilliseconds);

                return new SearchResponseDto
                {
                    Results = results,
                    TotalCount = response.Total,
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error performing advanced search");
                throw;
            }
        }

        public async Task<bool> DeleteByQueryAsync(string query)
        {
            Log.Information("Deleting documents matching query: {Query}", query);

            try
            {
                var response = await _elasticClient.DeleteByQueryAsync<SearchDocument>(d => d
                    .Index(_defaultIndex)
                    .Query(q => q.QueryString(qs => qs.Query(query)))
                );

                if (!response.IsValid)
                {
                    Log.Error("Delete by query failed: {Error}", response.ServerError?.Error?.Reason);
                    return false;
                }

                Log.Information("Delete by query completed: {Count} documents deleted", response.Deleted);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting by query");
                return false;
            }
        }

        // ==================== INDEX MANAGEMENT ====================
        public async Task<bool> CreateIndexAsync(string indexName)
        {
            Log.Information("Creating index: {IndexName}", indexName);

            try
            {
                var response = await _elasticClient.Indices.CreateAsync(indexName, c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0)
                    )
                    .Map<SearchDocument>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Keyword(k => k.Name(n => n.DocumentType))
                            .Keyword(k => k.Name(n => n.Category))
                            .Keyword(k => k.Name(n => n.Status))
                            .Text(t => t
                                .Name(n => n.Title)
                                .Analyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.Description)
                                .Analyzer("standard")
                            )
                            .Text(t => t
                                .Name(n => n.Content)
                                .Analyzer("standard")
                            )
                        )
                    )
                );

                if (!response.IsValid)
                {
                    Log.Error("Failed to create index: {Error}", response.ServerError?.Error?.Reason);
                    return false;
                }

                Log.Information("Index created successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating index");
                return false;
            }
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            Log.Information("Deleting index: {IndexName}", indexName);

            try
            {
                var response = await _elasticClient.Indices.DeleteAsync(indexName);

                if (!response.IsValid)
                {
                    Log.Error("Failed to delete index: {Error}", response.ServerError?.Error?.Reason);
                    return false;
                }

                Log.Information("Index deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting index");
                return false;
            }
        }

        public async Task<bool> IndexExistsAsync(string indexName)
        {
            try
            {
                var response = await _elasticClient.Indices.ExistsAsync(indexName);
                return response.Exists;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking if index exists");
                return false;
            }
        }

        // ==================== HELPER METHODS ====================
        private string GetIndexName(string documentType)
        {
            return $"search-{documentType.ToLower()}";
        }

        private SearchDescriptor<SearchDocument> ApplySorting(
            SearchDescriptor<SearchDocument> descriptor,
            string sortBy,
            string sortOrder)
        {
            var sortIsDescending = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "date" => descriptor.Sort(s => s
                    .Descending(f => f.CreatedAt)),
                "price" => descriptor.Sort(s => s
                    .Field(f => f.Price, sortIsDescending ? SortOrder.Descending : SortOrder.Ascending)),
                "relevance" => descriptor, // Default relevance sorting
                _ => descriptor
            };
        }
    }
}
