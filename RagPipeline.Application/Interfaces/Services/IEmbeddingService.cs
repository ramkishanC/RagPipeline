namespace RagPipeline.Application.Interfaces.Services
{
    public interface IEmbeddingService
    {
        Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default);

        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}
