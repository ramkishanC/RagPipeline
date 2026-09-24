namespace RagPipeline.Application.DTOs.Search;

public class HybridSearchResult
{
    public Guid DocumentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public Guid ChunkId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public int? VectorRank { get; set; }

    public double? VectorDistance { get; set; }

    public int? KeywordRank { get; set; }

    public float? KeywordScore { get; set; }

    public double RrfScore { get; set; }
}
