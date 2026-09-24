namespace RagPipeline.Application.DTOs.Search;

public class HybridSearchResponse
{
    public string Query { get; set; } = string.Empty;

    public List<HybridSearchResult> Results { get; set; } = [];
}
