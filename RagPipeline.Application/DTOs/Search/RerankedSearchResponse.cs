namespace RagPipeline.Application.DTOs.Search;

public class RerankedSearchResponse
{
    public string Query { get; set; } = string.Empty;

    public List<RerankedSearchResult> Results { get; set; } = [];
}
