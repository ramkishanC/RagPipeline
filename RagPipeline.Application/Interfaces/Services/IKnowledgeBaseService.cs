using RagPipeline.Application.DTOs.KnowledgeBases;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IKnowledgeBaseService
    {
        Task<KnowledgeBaseResponse> CreateAsync(CreateKnowledgeBaseRequest request, CancellationToken cancellationToken = default);

        Task<bool> HasAccessAsync(Guid knowledgeBaseId, string externalUserId, CancellationToken cancellationToken = default);
    }
}
