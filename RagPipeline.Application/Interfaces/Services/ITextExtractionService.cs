namespace RagPipeline.Application.Interfaces.Services
{
    public interface ITextExtractionService
    {
        Task<string> ExtractAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    }
}
