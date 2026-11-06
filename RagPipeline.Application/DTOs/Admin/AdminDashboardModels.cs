namespace RagPipeline.Application.DTOs.Admin
{
    public class RecordQueryTelemetryRequest
    {
        public Guid KnowledgeBaseId { get; set; }
        public Guid? ConversationId { get; set; }
        public string Query { get; set; } = string.Empty;
        public string RewrittenQuery { get; set; } = string.Empty;
        public long RewriteLatencyMs { get; set; }
        public long RetrievalLatencyMs { get; set; }
        public long LlmLatencyMs { get; set; }
        public long TotalLatencyMs { get; set; }
        public int RetrievedChunkCount { get; set; }
        public string PromptText { get; set; } = string.Empty;
        public string CompletionText { get; set; } = string.Empty;
        public bool IsNoAnswer { get; set; }
        public bool CitationsValid { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    public class QueryTelemetryResponse
    {
        public Guid Id { get; set; }
        public Guid? ConversationId { get; set; }
        public string Query { get; set; } = string.Empty;
        public string RewrittenQuery { get; set; } = string.Empty;
        public long RewriteLatencyMs { get; set; }
        public long RetrievalLatencyMs { get; set; }
        public long LlmLatencyMs { get; set; }
        public long TotalLatencyMs { get; set; }
        public int RetrievedChunkCount { get; set; }
        public int EstimatedPromptTokens { get; set; }
        public int EstimatedCompletionTokens { get; set; }
        public bool IsNoAnswer { get; set; }
        public bool CitationsValid { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminDashboardResponse
    {
        public Guid KnowledgeBaseId { get; set; }
        public int TotalDocumentsUploaded { get; set; }
        public int DocumentsCompleted { get; set; }
        public int DocumentsProcessingOrPending { get; set; }
        public int DocumentsFailed { get; set; }
        public int TotalEmbeddingCount { get; set; }
        public int TotalQueries { get; set; }
        public int FailedQueriesCount { get; set; }
        public int NoAnswerQueriesCount { get; set; }
        public double NoAnswerRate { get; set; }
        public double AvgRewriteLatencyMs { get; set; }
        public double AvgRetrievalLatencyMs { get; set; }
        public double AvgLlmLatencyMs { get; set; }
        public double AvgTotalLatencyMs { get; set; }
        public long TotalEstimatedPromptTokens { get; set; }
        public long TotalEstimatedCompletionTokens { get; set; }
        public long TotalEstimatedTokens { get; set; }
        public int TotalFeedbackCount { get; set; }
        public int HelpfulFeedbackCount { get; set; }
        public int UnhelpfulFeedbackCount { get; set; }
        public double HelpfulFeedbackRate { get; set; }
        public List<QueryTelemetryResponse> RecentNoAnswerQueries { get; set; } = new();
        public List<QueryTelemetryResponse> RecentFailedQueries { get; set; } = new();
    }
}
