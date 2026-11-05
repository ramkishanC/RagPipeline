using RagPipeline.Application.DTOs.Feedback;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IFeedbackService
    {
        Task<FeedbackResponse> SubmitFeedbackAsync(SubmitFeedbackRequest request, CancellationToken cancellationToken = default);
        Task<List<FeedbackResponse>> GetFeedbackAsync(Guid knowledgeBaseId, bool? onlyHelpful = null, CancellationToken cancellationToken = default);
        Task<FeedbackSummaryResponse> GetSummaryAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default);
    }
}
