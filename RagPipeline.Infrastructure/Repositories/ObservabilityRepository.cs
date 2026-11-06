using Microsoft.EntityFrameworkCore;
using RagPipeline.Application.DTOs.Admin;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Domain.Entities;
using RagPipeline.Domain.Enums;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.Infrastructure.Repositories
{
    public class ObservabilityRepository : IObservabilityRepository
    {
        private readonly RagDbContext _dbContext;

        public ObservabilityRepository(RagDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddTelemetryAsync(RagQueryTelemetry telemetry, CancellationToken cancellationToken = default)
        {
            await _dbContext.RagQueryTelemetries.AddAsync(telemetry, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<RagQueryTelemetry>> GetRecentTelemetriesAsync(Guid knowledgeBaseId, bool? onlyNoAnswer = null, int limit = 50, CancellationToken cancellationToken = default)
        {
            limit = Math.Clamp(limit, 1, 200);

            var query = _dbContext.RagQueryTelemetries
                .AsNoTracking()
                .Where(x => x.KnowledgeBaseId == knowledgeBaseId);

            if (onlyNoAnswer.HasValue)
            {
                query = query.Where(x => x.IsNoAnswer == onlyNoAnswer.Value);
            }

            return await query.OrderByDescending(x => x.CreatedAt).Take(limit).ToListAsync(cancellationToken);
        }

        public async Task<AdminDashboardResponse> GetDashboardMetricsAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            var documentStatuses = await _dbContext.Documents
                .AsNoTracking()
                .Where(d => d.KnowledgeBaseId == knowledgeBaseId)
                .Select(d => d.Status)
                .ToListAsync(cancellationToken);

            var totalEmbeddings = await _dbContext.DocumentChunks
                .AsNoTracking()
                .CountAsync(c => c.Document.KnowledgeBaseId == knowledgeBaseId, cancellationToken);

            var telemetries = await _dbContext.RagQueryTelemetries
                .AsNoTracking()
                .Where(t => t.KnowledgeBaseId == knowledgeBaseId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);

            var feedbacks = await _dbContext.AnswerFeedbacks
                .AsNoTracking()
                .Where(f => f.KnowledgeBaseId == knowledgeBaseId)
                .Select(f => f.IsHelpful)
                .ToListAsync(cancellationToken);

            var totalQueries = telemetries.Count;
            var noAnswerCount = telemetries.Count(t => t.IsNoAnswer);
            var helpfulCount = feedbacks.Count(f => f);
            var totalPromptTokens = telemetries.Sum(t => (long)t.EstimatedPromptTokens);
            var totalCompletionTokens = telemetries.Sum(t => (long)t.EstimatedCompletionTokens);

            return new AdminDashboardResponse
            {
                KnowledgeBaseId = knowledgeBaseId,
                TotalDocumentsUploaded = documentStatuses.Count,
                DocumentsCompleted = documentStatuses.Count(s => s == DocumentStatus.Completed),
                DocumentsFailed = documentStatuses.Count(s => s == DocumentStatus.Failed),
                DocumentsProcessingOrPending = documentStatuses.Count(s => s == DocumentStatus.Pending || s == DocumentStatus.Processing),
                TotalEmbeddingCount = totalEmbeddings,
                TotalQueries = totalQueries,
                FailedQueriesCount = telemetries.Count(t => !t.IsSuccess),
                NoAnswerQueriesCount = noAnswerCount,
                NoAnswerRate = totalQueries > 0 ? Math.Round((double)noAnswerCount / totalQueries, 4) : 0.0,
                AvgRewriteLatencyMs = totalQueries > 0 ? Math.Round(telemetries.Average(t => t.RewriteLatencyMs), 2) : 0.0,
                AvgRetrievalLatencyMs = totalQueries > 0 ? Math.Round(telemetries.Average(t => t.RetrievalLatencyMs), 2) : 0.0,
                AvgLlmLatencyMs = totalQueries > 0 ? Math.Round(telemetries.Average(t => t.LlmLatencyMs), 2) : 0.0,
                AvgTotalLatencyMs = totalQueries > 0 ? Math.Round(telemetries.Average(t => t.TotalLatencyMs), 2) : 0.0,
                TotalEstimatedPromptTokens = totalPromptTokens,
                TotalEstimatedCompletionTokens = totalCompletionTokens,
                TotalEstimatedTokens = totalPromptTokens + totalCompletionTokens,
                TotalFeedbackCount = feedbacks.Count,
                HelpfulFeedbackCount = helpfulCount,
                UnhelpfulFeedbackCount = feedbacks.Count - helpfulCount,
                HelpfulFeedbackRate = feedbacks.Count > 0 ? Math.Round((double)helpfulCount / feedbacks.Count, 4) : 0.0,
                RecentNoAnswerQueries = telemetries.Where(t => t.IsNoAnswer).Take(10).Select(MapToDto).ToList(),
                RecentFailedQueries = telemetries.Where(t => !t.IsSuccess).Take(10).Select(MapToDto).ToList()
            };
        }

        private static QueryTelemetryResponse MapToDto(RagQueryTelemetry telemetry)
        {
            return new QueryTelemetryResponse
            {
                Id = telemetry.Id,
                ConversationId = telemetry.ConversationId,
                Query = telemetry.Query,
                RewrittenQuery = telemetry.RewrittenQuery,
                RewriteLatencyMs = telemetry.RewriteLatencyMs,
                RetrievalLatencyMs = telemetry.RetrievalLatencyMs,
                LlmLatencyMs = telemetry.LlmLatencyMs,
                TotalLatencyMs = telemetry.TotalLatencyMs,
                RetrievedChunkCount = telemetry.RetrievedChunkCount,
                EstimatedPromptTokens = telemetry.EstimatedPromptTokens,
                EstimatedCompletionTokens = telemetry.EstimatedCompletionTokens,
                IsNoAnswer = telemetry.IsNoAnswer,
                CitationsValid = telemetry.CitationsValid,
                IsSuccess = telemetry.IsSuccess,
                ErrorMessage = telemetry.ErrorMessage,
                CreatedAt = telemetry.CreatedAt
            };
        }
    }
}
