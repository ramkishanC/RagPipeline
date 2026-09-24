using System.Net.Http.Json;
using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Infrastructure.Models.Reranker;

namespace RagPipeline.Infrastructure.Services;

public class RerankerService : IRerankerService
{
    private readonly HttpClient _httpClient;

    public RerankerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<RerankScore>> RerankAsync(string query, IReadOnlyList<RerankCandidate> candidates, CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var request = new RerankerRequest
        {
            Query = query,
            Documents = candidates.Select(x => new RerankerDocument
            {
                Id = x.ChunkId.ToString(),
                Text = x.Content
            }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("rerank", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RerankerResponse>(cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("Reranker returned an invalid response.");
        }

        return result.Results.Select(x => new RerankScore
        {
            ChunkId = Guid.Parse(x.Id),
            Score = x.Score
        }).ToList();
    }
}
