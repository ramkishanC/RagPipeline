namespace RagPipeline.Domain.Entities
{
    public class AnswerFeedback
    {
        public Guid Id { get; set; }
        public Guid KnowledgeBaseId { get; set; }
        public Guid? ConversationId { get; set; }
        public Guid? ChatMessageId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? SourcesJson { get; set; }
        public bool IsHelpful { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public KnowledgeBase KnowledgeBase { get; set; } = null!;
    }
}