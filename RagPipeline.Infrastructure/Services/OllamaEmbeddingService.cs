using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Infrastructure.Models.Ollama;

namespace RagPipeline.Infrastructure.Services
{
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public OllamaEmbeddingService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _model = configuration["Ollama:EmbeddingModel"]
                ?? throw new InvalidOperationException(
                    "Ollama embedding model is not configured.");
        }

        public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync( IReadOnlyList<string> texts,  CancellationToken cancellationToken = default)
        {
            if (texts.Count == 0)
            {
                return [];
            }

            var request = new OllamaEmbedRequest
            {
                Model = _model,
                Input = texts.ToArray()
            };

            var response = await _httpClient.PostAsJsonAsync("api/embed",request,cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken);

            if (result is null || result.Embeddings.Length == 0)
            {
                throw new InvalidOperationException("Ollama returned no embeddings.");
            }

            return result.Embeddings;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            var embeddings = await GenerateEmbeddingsAsync([text],cancellationToken);

            return embeddings[0];
        }
    }
}
