using RagPipeline.Application.DTOs.Documents;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<DocumentResponse> UploadAsync(Guid knowledgeBaseId, Stream stream, string fileName, string contentType, long fileSize, string? category, List<string>? tags, CancellationToken cancellationToken = default);
        Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<DocumentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<DocumentResponse>> GetAllAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
