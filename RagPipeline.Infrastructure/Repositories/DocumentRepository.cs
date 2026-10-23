using Microsoft.EntityFrameworkCore;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Domain.Entities;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.Infrastructure.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly RagDbContext _dbContext;

        public DocumentRepository(RagDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
        {
            await _dbContext.Documents.AddAsync(document, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Document>> GetAllByKnowledgeBaseIdAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Documents.AsNoTracking().Where(x => x.KnowledgeBaseId == knowledgeBaseId).OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        }

        public async Task<bool> KnowledgeBaseExistsAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.KnowledgeBases.AsNoTracking().AnyAsync(x => x.Id == knowledgeBaseId, cancellationToken);
        }

        public async Task DeleteAsync(Document document, CancellationToken cancellationToken = default)
        {
            _dbContext.Documents.Remove(document);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
