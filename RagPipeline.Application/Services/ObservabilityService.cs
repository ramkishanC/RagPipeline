using Microsoft.Extensions.Logging;
using RagPipeline.Application.DTOs.Admin;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Services
{
    public class ObservabilityService : IObservabilityService
    {
        private readonly IObservabilityRepository _observabilityRepository;
        private readonly ILogger<ObservabilityService> _logger;

        public ObservabilityService(IObservabilityRepository observabilityRepository, ILogger<ObservabilityService> logger)
        {
            _observabilityRepository = observabilityRepository;
            _logger = logger;
        }

        public async Task RecordQueryTelemetryAsync(RecordQueryTelemetryRequest request, CancellationToken cancellationToken = default)
        {
            var promptTokens = EstimateTokens(request.PromptText);
            var completionTokens = EstimateTokens(request.CompletionText);
            var entity = new RagQueryTelemetry
            {
                Id = Guid.NewGuid(),
                KnowledgeBaseId = request.KnowledgeBaseId,
                ConversationId = request.ConversationId,
                Query = request.Query,
                RewrittenQuery = request.RewrittenQuery,
                RewriteLatencyMs = request.RewriteLatencyMs,
                RetrievalLatencyMs = request.RetrievalLatencyMs,
                LlmLatencyMs = request.LlmLatencyMs,
                TotalLatencyMs = request.TotalLatencyMs,
                RetrievedChunkCount = request.RetrievedChunkCount,
                EstimatedPromptTokens = promptTokens,
                EstimatedCompletionTokens = completionTokens,
                IsNoAnswer = request.IsNoAnswer,
                CitationsValid = request.CitationsValid,
                IsSuccess = request.IsSuccess,
                ErrorMessage = request.ErrorMessage,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("RAG Query Telemetry | KB: {KnowledgeBaseId} | RewriteMs: {RewriteLatencyMs} | RetrievalMs: {RetrievalLatencyMs} | LlmMs: {LlmLatencyMs} | TotalMs: {TotalLatencyMs} | Chunks: {RetrievedChunkCount} | Tokens: {TotalTokens} | NoAnswer: {IsNoAnswer} | CitationsValid: {CitationsValid} | IsSuccess: {IsSuccess}", entity.KnowledgeBaseId, entity.RewriteLatencyMs, entity.RetrievalLatencyMs, entity.LlmLatencyMs, entity.TotalLatencyMs, entity.RetrievedChunkCount, promptTokens + completionTokens, entity.IsNoAnswer, entity.CitationsValid, entity.IsSuccess);
            await _observabilityRepository.AddTelemetryAsync(entity, cancellationToken);
        }

        public async Task<AdminDashboardResponse> GetDashboardAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            return await _observabilityRepository.GetDashboardMetricsAsync(knowledgeBaseId, cancellationToken);
        }

        public async Task<List<QueryTelemetryResponse>> GetRecentQueriesAsync(Guid knowledgeBaseId, bool? onlyNoAnswer = null, int limit = 50, CancellationToken cancellationToken = default)
        {
            var items = await _observabilityRepository.GetRecentTelemetriesAsync(knowledgeBaseId, onlyNoAnswer, limit, cancellationToken);
            return items.Select(MapToDto).ToList();
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

        private static int EstimateTokens(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
        }
    }
}
