using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Services;

public interface IHybridSearchService
{
    Task<HybridSearchResponse> SearchAsync(Guid knowledgeBaseId, string query, int topK = 5, CancellationToken cancellationToken = default);
}
