using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Services;

public interface IKeywordSearchService
{
    Task<KeywordSearchResponse> SearchAsync(Guid knowledgeBaseId, string query, int topK = 5, string? category = null, List<string>? tags = null, CancellationToken cancellationToken = default);
}
