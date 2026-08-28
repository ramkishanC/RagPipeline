using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.Application.Services
{
    public class VectorSearchService : IVectorSearchService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorSearchRepository _vectorSearchRepository;

        public VectorSearchService(
            IEmbeddingService embeddingService,
            IVectorSearchRepository vectorSearchRepository)
        {
            _embeddingService = embeddingService;
            _vectorSearchRepository = vectorSearchRepository;
        }

        public async Task<VectorSearchResponse> SearchAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Search query is required.",
                    nameof(query));
            }

            if (topK <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(topK),"TopK must be greater than zero.");
            }

            if (topK > 20)
            {
                topK = 20;
            }

            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query,cancellationToken);

            if (queryEmbedding.Length != 768)
            {
                throw new InvalidOperationException($"Expected 768-dimensional embedding, but received {queryEmbedding.Length}.");
            }

            var results = await _vectorSearchRepository.SearchAsync(queryEmbedding,topK,cancellationToken);

            return new VectorSearchResponse
            {
                Query = query,
                Results = results
            };
        }
    }
}
