namespace RagPipeline.Infrastructure.Models.Ollama
{
    public class OllamaEmbedRequest
    {
        public string Model { get; set; } = string.Empty;

        public string[] Input { get; set; } = [];
    }
}
