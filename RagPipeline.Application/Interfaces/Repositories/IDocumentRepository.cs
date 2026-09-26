using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Interfaces.Repositories
{
    public interface IDocumentRepository
    {
        Task AddAsync(Document document, CancellationToken cancellationToken = default);
        Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Document>> GetAllByKnowledgeBaseIdAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default);
        Task<bool> KnowledgeBaseExistsAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default);
    }
}
