namespace RagPipeline.Domain.Entities
{
    public class RagQueryTelemetry
    {
        public Guid Id { get; set; }
        public Guid KnowledgeBaseId { get; set; }
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

        public KnowledgeBase KnowledgeBase { get; set; } = null!;
    }
}