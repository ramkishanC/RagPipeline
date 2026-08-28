namespace RagPipeline.Application.DTOs.Search
{
    public class VectorSearchResponse
    {
        public string Query { get; set; } = string.Empty;

        public List<VectorSearchResult> Results { get; set; } = [];
    }
}
