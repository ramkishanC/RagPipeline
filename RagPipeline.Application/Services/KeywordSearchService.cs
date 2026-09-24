using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.Application.Services;

public class KeywordSearchService : IKeywordSearchService
{
    private readonly IKeywordSearchRepository _keywordSearchRepository;

    public KeywordSearchService(IKeywordSearchRepository keywordSearchRepository)
    {
        _keywordSearchRepository = keywordSearchRepository;
    }

    public async Task<KeywordSearchResponse> SearchAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Search query is required.", nameof(query));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        topK = Math.Min(topK, 20);

        var results = await _keywordSearchRepository.SearchAsync(query, topK, cancellationToken);

        return new KeywordSearchResponse
        {
            Query = query,
            Results = results
        };
    }
}
