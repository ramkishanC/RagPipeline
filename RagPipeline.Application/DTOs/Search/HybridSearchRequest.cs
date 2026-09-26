namespace RagPipeline.Application.DTOs.Search;

public class HybridSearchRequest
{
    public Guid KnowledgeBaseId { get; set; }

    public string Query { get; set; } = string.Empty;

    public int TopK { get; set; } = 5;
}
