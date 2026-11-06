using Microsoft.EntityFrameworkCore;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Domain.Entities;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.Infrastructure.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly RagDbContext _dbContext;

        public FeedbackRepository(RagDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AnswerFeedback> AddAsync(AnswerFeedback feedback, CancellationToken cancellationToken = default)
        {
            await _dbContext.AnswerFeedbacks.AddAsync(feedback, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return feedback;
        }

        public async Task<List<AnswerFeedback>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId, bool? onlyHelpful = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.AnswerFeedbacks
                .AsNoTracking()
                .Where(x => x.KnowledgeBaseId == knowledgeBaseId);

            if (onlyHelpful.HasValue)
            {
                query = query.Where(x => x.IsHelpful == onlyHelpful.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
