using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Interfaces.Repositories
{
    public interface IFeedbackRepository
    {
        Task<AnswerFeedback> AddAsync(AnswerFeedback feedback, CancellationToken cancellationToken = default);
        Task<List<AnswerFeedback>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId, bool? onlyHelpful = null, CancellationToken cancellationToken = default);
    }
}
