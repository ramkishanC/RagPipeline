using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.Infrastructure.Repositories;

public class KeywordSearchRepository : IKeywordSearchRepository
{
    private readonly RagDbContext _dbContext;

    public KeywordSearchRepository(RagDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<KeywordSearchResult>> SearchAsync(Guid knowledgeBaseId, string query, int topK, string? category = null, List<string>? tags = null, CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.DocumentChunks
            .AsNoTracking()
            .Where(x => x.Document.KnowledgeBaseId == knowledgeBaseId && EF.Property<NpgsqlTsVector>(x, "SearchVector").Matches(EF.Functions.WebSearchToTsQuery("english", query)));

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

        return await queryable
            .OrderByDescending(x => EF.Property<NpgsqlTsVector>(x, "SearchVector").Rank(EF.Functions.WebSearchToTsQuery("english", query)))
            .Take(topK)
            .Select(x => new KeywordSearchResult
            {
                DocumentId = x.DocumentId,
                FileName = x.Document.FileName,
                ChunkId = x.Id,
                ChunkIndex = x.ChunkIndex,
                Content = x.Content,
                Rank = EF.Property<NpgsqlTsVector>(x, "SearchVector").Rank(EF.Functions.WebSearchToTsQuery("english", query))
            })
            .ToListAsync(cancellationToken);
    }
}
