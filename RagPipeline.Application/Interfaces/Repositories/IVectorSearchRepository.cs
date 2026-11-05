using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Repositories
{
    public interface IVectorSearchRepository
    {
        Task<List<VectorSearchResult>> SearchAsync(Guid knowledgeBaseId, float[] queryEmbedding, int topK, string? category = null, List<string>? tags = null, CancellationToken cancellationToken = default);
    }
}
