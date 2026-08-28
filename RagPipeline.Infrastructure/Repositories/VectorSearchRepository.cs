using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.Infrastructure.Repositories
{
    public class VectorSearchRepository : IVectorSearchRepository
    {
        private readonly RagDbContext _dbContext;

        public VectorSearchRepository(RagDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<VectorSearchResult>> SearchAsync( float[] queryEmbedding, int topK, CancellationToken cancellationToken = default)
        {
            var queryVector = new Vector(queryEmbedding);

            var results = await _dbContext.DocumentChunks
                .AsNoTracking()
                .Where(x => x.Embedding != null)
                .OrderBy(x => x.Embedding!.CosineDistance(queryVector))
                .Take(topK)
                .Select(x => new VectorSearchResult
                {
                    DocumentId = x.DocumentId,
                    FileName = x.Document.FileName,
                    ChunkId = x.Id,
                    ChunkIndex = x.ChunkIndex,
                    Content = x.Content,
                    Distance = x.Embedding!.CosineDistance(queryVector)
                })
                .ToListAsync(cancellationToken);

            return results;
        }
    }
}
