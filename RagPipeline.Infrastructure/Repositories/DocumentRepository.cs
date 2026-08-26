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

        public async Task<List<Document>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Documents.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        }
    }
}
