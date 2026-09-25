namespace RagPipeline.Application.DTOs.Search;

public class KeywordSearchRequest
{
    public Guid KnowledgeBaseId { get; set; }

    public string Query { get; set; } = string.Empty;

    public int TopK { get; set; } = 5;
}
