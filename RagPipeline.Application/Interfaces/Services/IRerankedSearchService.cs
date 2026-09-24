using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Services;

public interface IRerankedSearchService
{
    Task<RerankedSearchResponse> SearchAsync(string query, int topK = 5, CancellationToken cancellationToken = default);
}
