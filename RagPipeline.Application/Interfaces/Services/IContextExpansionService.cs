using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IContextExpansionService
    {
        Task<List<string>> BuildExpandedContextAsync( IReadOnlyList<RerankedSearchResult> rankedResults, int windowSize = 1, CancellationToken cancellationToken = default);
    }
}
