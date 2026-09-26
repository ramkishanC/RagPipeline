using RagPipeline.Application.DTOs.KnowledgeBases;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Services
{
    public class KnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly IKnowledgeBaseRepository _knowledgeBaseRepository;

        public KnowledgeBaseService(IKnowledgeBaseRepository knowledgeBaseRepository)
        {
            _knowledgeBaseRepository = knowledgeBaseRepository;
        }

        public async Task<KnowledgeBaseResponse> CreateAsync(CreateKnowledgeBaseRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _knowledgeBaseRepository.GetRagUserByExternalUserIdAsync(request.ExternalUserId, cancellationToken);

            if (user is null)
            {
                user = new RagUser
                {
                    Id = Guid.NewGuid(),
                    ExternalUserId = request.ExternalUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _knowledgeBaseRepository.AddRagUserAsync(user, cancellationToken);
            }

            var knowledgeBase = new KnowledgeBase
            {
                Id = Guid.NewGuid(),
                RagUserId = user.Id,
                Name = request.Name.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _knowledgeBaseRepository.AddAsync(knowledgeBase, cancellationToken);

            return new KnowledgeBaseResponse
            {
                Id = knowledgeBase.Id,
                ExternalUserId = user.ExternalUserId,
                Name = knowledgeBase.Name,
                CreatedAt = knowledgeBase.CreatedAt
            };
        }

        public async Task<bool> HasAccessAsync(Guid knowledgeBaseId, string externalUserId, CancellationToken cancellationToken = default)
        {
            return await _knowledgeBaseRepository.GetByIdForUserAsync(knowledgeBaseId, externalUserId, cancellationToken) is not null;
        }
    }
}
