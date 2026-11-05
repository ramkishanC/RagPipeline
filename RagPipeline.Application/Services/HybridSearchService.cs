using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.Application.Services;

public class HybridSearchService : IHybridSearchService
{
    private const int RrfK = 60;

    private readonly IVectorSearchService _vectorSearchService;
    private readonly IKeywordSearchService _keywordSearchService;

    public HybridSearchService(IVectorSearchService vectorSearchService, IKeywordSearchService keywordSearchService)
    {
        _vectorSearchService = vectorSearchService;
        _keywordSearchService = keywordSearchService;
    }

    public async Task<HybridSearchResponse> SearchAsync(
        Guid knowledgeBaseId,
        string query,
        int topK = 5,
        string? category = null,
        List<string>? tags = null,
        double vectorWeight = 1.0,
        double keywordWeight = 1.0,
        CancellationToken cancellationToken = default)
    {
        if (knowledgeBaseId == Guid.Empty)
        {
            throw new ArgumentException("Knowledge base ID is required.", nameof(knowledgeBaseId));
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Search query is required.", nameof(query));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        vectorWeight = Math.Clamp(vectorWeight, 0.0, 5.0);
        keywordWeight = Math.Clamp(keywordWeight, 0.0, 5.0);

        topK = Math.Min(topK, 20);

        var candidateK = Math.Min(Math.Max(topK * 4, 20), 50);
        var vectorResponse = await _vectorSearchService.SearchAsync(knowledgeBaseId, query, candidateK, category, tags, cancellationToken); 
        var keywordResponse = await _keywordSearchService.SearchAsync(knowledgeBaseId, query, candidateK, category, tags, cancellationToken);
        var fused = new Dictionary<Guid, HybridSearchResult>();

        for (var i = 0; i < vectorResponse.Results.Count; i++)
        {
            var result = vectorResponse.Results[i];
            var rank = i + 1;
            var rrfContribution = vectorWeight / (RrfK + rank);

            fused[result.ChunkId] = new HybridSearchResult
            {
                DocumentId = result.DocumentId,
                FileName = result.FileName,
                ChunkId = result.ChunkId,
                ChunkIndex = result.ChunkIndex,
                Content = result.Content,
                VectorRank = rank,
                VectorDistance = result.Distance,
                RrfScore = rrfContribution
            };
        }

        for (var i = 0; i < keywordResponse.Results.Count; i++)
        {
            var result = keywordResponse.Results[i];
            var rank = i + 1;
            var rrfContribution = keywordWeight / (RrfK + rank);

            if (fused.TryGetValue(result.ChunkId, out var existing))
            {
                existing.KeywordRank = rank;
                existing.KeywordScore = result.Rank;
                existing.RrfScore += rrfContribution;
            }
            else
            {
                fused[result.ChunkId] = new HybridSearchResult
                {
                    DocumentId = result.DocumentId,
                    FileName = result.FileName,
                    ChunkId = result.ChunkId,
                    ChunkIndex = result.ChunkIndex,
                    Content = result.Content,
                    KeywordRank = rank,
                    KeywordScore = result.Rank,
                    RrfScore = rrfContribution
                };
            }
        }

        var finalResults = fused.Values
            .OrderByDescending(x => x.RrfScore)
            .ThenBy(x => x.VectorDistance ?? double.MaxValue)
            .Take(topK)
            .ToList();

        return new HybridSearchResponse
        {
            Query = query,
            Results = finalResults
        };
    }
}
