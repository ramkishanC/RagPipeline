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

        public async Task<List<VectorSearchResult>> SearchAsync(Guid knowledgeBaseId, float[] queryEmbedding, int topK, string? category = null, List<string>? tags = null, CancellationToken cancellationToken = default)
        {
            var queryVector = new Vector(queryEmbedding);

            var queryable = _dbContext.DocumentChunks
                .AsNoTracking()
                .Where(x => x.Document.KnowledgeBaseId == knowledgeBaseId && x.Embedding != null);

            if (!string.IsNullOrWhiteSpace(category))
            {
                queryable = queryable.Where(x => x.Document.Category == category);
            }

            if (tags != null && tags.Count > 0)
            {
                foreach (var tag in tags)
                {
                    queryable = queryable.Where(x => x.Document.Tags.Contains(tag));
                }
            }

            var results = await queryable
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
