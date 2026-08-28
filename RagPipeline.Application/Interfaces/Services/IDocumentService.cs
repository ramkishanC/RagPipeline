using RagPipeline.Application.DTOs.Documents;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<DocumentResponse> UploadAsync(Stream stream, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default);
        Task<DocumentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<DocumentResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
