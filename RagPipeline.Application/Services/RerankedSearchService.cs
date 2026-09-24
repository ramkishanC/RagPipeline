using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.Application.Services;

public class RerankedSearchService : IRerankedSearchService
{
    private readonly IHybridSearchService _hybridSearchService;
    private readonly IRerankerService _rerankerService;

    public RerankedSearchService(IHybridSearchService hybridSearchService, IRerankerService rerankerService)
    {
        _hybridSearchService = hybridSearchService;
        _rerankerService = rerankerService;
    }

    public async Task<RerankedSearchResponse> SearchAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query is required.", nameof(query));
        }

        topK = Math.Clamp(topK, 1, 20);

        const int candidateCount = 20;

        var hybrid = await _hybridSearchService.SearchAsync(query, candidateCount, cancellationToken);
        var candidates = hybrid.Results.Select(x => new RerankCandidate
        {
            ChunkId = x.ChunkId,
            Content = x.Content
        }).ToList();
        var rerankScores = await _rerankerService.RerankAsync(query, candidates, cancellationToken);
        var scoreLookup = rerankScores.ToDictionary(x => x.ChunkId, x => x.Score);
        var finalResults = hybrid.Results
            .Where(x => scoreLookup.ContainsKey(x.ChunkId))
            .Select(x => new RerankedSearchResult
            {
                DocumentId = x.DocumentId,
                FileName = x.FileName,
                ChunkId = x.ChunkId,
                ChunkIndex = x.ChunkIndex,
                Content = x.Content,
                RrfScore = x.RrfScore,
                RerankScore = scoreLookup[x.ChunkId]
            })
            .OrderByDescending(x => x.RerankScore)
            .Take(topK)
            .ToList();

        return new RerankedSearchResponse
        {
            Query = query,
            Results = finalResults
        };
    }
}
