using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Services;

public interface IRerankedSearchService
{
    Task<RerankedSearchResponse> SearchAsync( Guid knowledgeBaseId, string query, int topK = 5, string? category = null, List<string>? tags = null, double vectorWeight = 1.0, double keywordWeight = 1.0,  CancellationToken cancellationToken = default);
}
