using Microsoft.EntityFrameworkCore;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Domain.Entities;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.Infrastructure.Repositories
{
    public class KnowledgeBaseRepository : IKnowledgeBaseRepository
    {
        private readonly RagDbContext _dbContext;

        public KnowledgeBaseRepository(RagDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RagUser?> GetRagUserByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.RagUsers.FirstOrDefaultAsync(x => x.ExternalUserId == externalUserId, cancellationToken);
        }

        public async Task<KnowledgeBase?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.KnowledgeBases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<KnowledgeBase?> GetByIdForUserAsync(Guid id, string externalUserId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.KnowledgeBases.AsNoTracking().Include(x => x.RagUser).FirstOrDefaultAsync(x => x.Id == id && x.RagUser.ExternalUserId == externalUserId, cancellationToken);
        }

        public async Task AddRagUserAsync(RagUser ragUser, CancellationToken cancellationToken = default)
        {
            await _dbContext.RagUsers.AddAsync(ragUser, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddAsync(KnowledgeBase knowledgeBase, CancellationToken cancellationToken = default)
        {
            await _dbContext.KnowledgeBases.AddAsync(knowledgeBase, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<KnowledgeBase>> GetAllForUserAsync(string externalUserId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.KnowledgeBases
                .AsNoTracking()
                .Include(x => x.RagUser)
                .Where(x => x.RagUser.ExternalUserId == externalUserId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(KnowledgeBase knowledgeBase, CancellationToken cancellationToken = default)
        {
            _dbContext.KnowledgeBases.Update(knowledgeBase);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(KnowledgeBase knowledgeBase, CancellationToken cancellationToken = default)
        {
            _dbContext.KnowledgeBases.Remove(knowledgeBase);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
