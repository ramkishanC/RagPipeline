using RagPipeline.Application.DTOs.Search;

namespace RagPipeline.Application.DTOs.Feedback
{
    public class SubmitFeedbackRequest
    {
        public Guid KnowledgeBaseId { get; set; }
        public Guid? ConversationId { get; set; }
        public Guid? ChatMessageId { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public List<AskSource>? Sources { get; set; }
        public bool IsHelpful { get; set; }
        public string? Comment { get; set; }
    }

    public class FeedbackResponse
    {
        public Guid Id { get; set; }
        public Guid KnowledgeBaseId { get; set; }
        public Guid? ConversationId { get; set; }
        public Guid? ChatMessageId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public List<AskSource> Sources { get; set; } = new();
        public bool IsHelpful { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FeedbackSummaryResponse
    {
        public Guid KnowledgeBaseId { get; set; }
        public int TotalFeedbackCount { get; set; }
        public int HelpfulCount { get; set; }
        public int UnhelpfulCount { get; set; }
        public double HelpfulRate { get; set; }
        public List<FeedbackResponse> RecentUnhelpful { get; set; } = new();
    }
}
