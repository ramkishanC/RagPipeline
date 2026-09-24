namespace RagPipeline.Infrastructure.Models.Reranker;

public class RerankerResponse
{
    public List<RerankerResult> Results { get; set; } = [];
}

public class RerankerResult
{
    public string Id { get; set; } = string.Empty;

    public double Score { get; set; }
}
