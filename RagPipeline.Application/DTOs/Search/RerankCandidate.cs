namespace RagPipeline.Application.DTOs.Search;

public class RerankCandidate
{
    public Guid ChunkId { get; set; }

    public string Content { get; set; } = string.Empty;
}
