using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Services;

public interface IKeywordSearchService
{
    Task<KeywordSearchResponse> SearchAsync(string query, int topK = 5, CancellationToken cancellationToken = default);
    Task<KeywordSearchResponse> SearchAsync(Guid knowledgeBaseId, string query, int topK = 5, CancellationToken cancellationToken = default);
}
