namespace RagPipeline.Infrastructure.Models.Reranker;

public class RerankerRequest
{
    public string Query { get; set; } = string.Empty;

    public List<RerankerDocument> Documents { get; set; } = [];
}

public class RerankerDocument
{
    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}
