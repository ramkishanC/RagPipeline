using RagPipeline.Application.DTOs.KnowledgeBases;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IKnowledgeBaseService
    {
        Task<KnowledgeBaseResponse> CreateAsync(string externalUserId, string name, CancellationToken cancellationToken = default);

        Task<bool> HasAccessAsync(Guid knowledgeBaseId, string externalUserId, CancellationToken cancellationToken = default);

        Task<List<KnowledgeBaseResponse>> GetAllAsync(string externalUserId, CancellationToken cancellationToken = default);

        Task<KnowledgeBaseResponse?> GetByIdAsync(Guid id, string externalUserId, CancellationToken cancellationToken = default);

        Task<KnowledgeBaseResponse?> UpdateAsync(Guid id, string externalUserId, UpdateKnowledgeBaseRequest request, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(Guid id, string externalUserId, CancellationToken cancellationToken = default);
    }
}
