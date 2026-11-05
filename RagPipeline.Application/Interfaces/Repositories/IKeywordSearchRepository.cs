using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Repositories;

public interface IKeywordSearchRepository
{
    Task<List<KeywordSearchResult>> SearchAsync(Guid knowledgeBaseId, string query, int topK, string? category = null, List<string>? tags = null, CancellationToken cancellationToken = default);
}
