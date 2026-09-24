namespace RagPipeline.Application.DTOs.Search;

public class KeywordSearchResponse
{
    public string Query { get; set; } = string.Empty;

    public List<KeywordSearchResult> Results { get; set; } = [];
}
