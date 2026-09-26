using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Interfaces.Repositories
{
    public interface IKnowledgeBaseRepository
    {
        Task<RagUser?> GetRagUserByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken = default);

        Task<KnowledgeBase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<KnowledgeBase?> GetByIdForUserAsync(Guid id, string externalUserId, CancellationToken cancellationToken = default);

        Task AddRagUserAsync(RagUser ragUser, CancellationToken cancellationToken = default);

        Task AddAsync(KnowledgeBase knowledgeBase, CancellationToken cancellationToken = default);
    }
}
