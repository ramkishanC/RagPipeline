namespace RagPipeline.Infrastructure.Models.Ollama
{
    public class OllamaEmbedResponse
    {
        public string Model { get; set; } = string.Empty;

        public float[][] Embeddings { get; set; } = [];
    }
}
