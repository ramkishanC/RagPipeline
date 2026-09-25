using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IVectorSearchService
    {
        Task<VectorSearchResponse> SearchAsync(Guid knowledgeBaseId, string query, int topK = 5, CancellationToken cancellationToken = default);
    }
}
