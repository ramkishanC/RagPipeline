using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Repositories;

public interface IKeywordSearchRepository
{
    Task<List<KeywordSearchResult>> SearchAsync(string query, int topK, CancellationToken cancellationToken = default);
    Task<List<KeywordSearchResult>> SearchAsync(Guid knowledgeBaseId, string query, int topK, CancellationToken cancellationToken = default);
}
