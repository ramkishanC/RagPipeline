using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.Application.Services
{
    public class ContextExpansionService : IContextExpansionService
    {
        private readonly IVectorSearchRepository _vectorSearchRepository;

        public ContextExpansionService(IVectorSearchRepository vectorSearchRepository)
        {
            _vectorSearchRepository = vectorSearchRepository;
        }

        public async Task<List<string>> BuildExpandedContextAsync( IReadOnlyList<RerankedSearchResult> rankedResults, int windowSize = 1, CancellationToken cancellationToken = default)
        {
            windowSize = Math.Clamp(windowSize, 0, 2);

            if (windowSize == 0 || rankedResults.Count == 0)
            {
                return rankedResults
                    .Select((x, index) => $"[{index + 1}] {x.Content}")
                    .ToList();
            }

            var expandedContexts = new List<string>(rankedResults.Count);

            for (var i = 0; i < rankedResults.Count; i++)
            {
                var target = rankedResults[i];
                var firstIndex = Math.Max(0, target.ChunkIndex - windowSize);
                var lastIndex = target.ChunkIndex + windowSize;
                var desiredIndices = Enumerable.Range(firstIndex, lastIndex - firstIndex + 1);

                var neighborChunks = await _vectorSearchRepository.GetChunksByDocumentAndIndicesAsync(target.DocumentId, desiredIndices, cancellationToken);

                var mergedText = neighborChunks.Count > 0
                    ? string.Join("\n", neighborChunks.Select(c => c.Content))
                    : target.Content;

                expandedContexts.Add($"[{i + 1}] {mergedText}");
            }

            return expandedContexts;
        }
    }
}
