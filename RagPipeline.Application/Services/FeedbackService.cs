using RagPipeline.Application.DTOs.Feedback;
using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;
using System.Text.Json;

namespace RagPipeline.Application.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IConversationRepository _conversationRepository;

        public FeedbackService(
            IFeedbackRepository feedbackRepository,
            IConversationRepository conversationRepository)
        {
            _feedbackRepository = feedbackRepository;
            _conversationRepository = conversationRepository;
        }

        public async Task<FeedbackResponse> SubmitFeedbackAsync(SubmitFeedbackRequest request, CancellationToken cancellationToken = default)
        {
            var question = request.Question?.Trim() ?? string.Empty;
            var answer = request.Answer?.Trim() ?? string.Empty;
            string? sourcesJson = request.Sources != null ? JsonSerializer.Serialize(request.Sources) : null;

            if (request.ConversationId.HasValue && request.ChatMessageId.HasValue)
            {
                var messages = await _conversationRepository.GetAllMessagesAsync(request.ConversationId.Value, cancellationToken);
                var targetIndex = messages.FindIndex(m => m.Id == request.ChatMessageId.Value);

                if (targetIndex >= 0)
                {
                    var assistantMsg = messages[targetIndex];
                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        answer = assistantMsg.Content;
                    }

                    if (string.IsNullOrWhiteSpace(sourcesJson))
                    {
                        sourcesJson = assistantMsg.CitationsJson;
                    }

                    for (var i = targetIndex - 1; i >= 0; i--)
                    {
                        if (messages[i].Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrWhiteSpace(question))
                            {
                                question = messages[i].Content;
                            }
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
            {
                throw new ArgumentException("Question and Answer must be provided or resolvable from ConversationId and ChatMessageId.");
            }

            var entity = new AnswerFeedback
            {
                Id = Guid.NewGuid(),
                KnowledgeBaseId = request.KnowledgeBaseId,
                ConversationId = request.ConversationId,
                ChatMessageId = request.ChatMessageId,
                Question = question,
                Answer = answer,
                SourcesJson = sourcesJson,
                IsHelpful = request.IsHelpful,
                Comment = request.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.AddAsync(entity, cancellationToken);
            return MapToResponse(entity);
        }

        public async Task<List<FeedbackResponse>> GetFeedbackAsync(Guid knowledgeBaseId, bool? onlyHelpful = null, CancellationToken cancellationToken = default)
        {
            var items = await _feedbackRepository.GetByKnowledgeBaseIdAsync(knowledgeBaseId, onlyHelpful, cancellationToken);
            return items.Select(MapToResponse).ToList();
        }

        public async Task<FeedbackSummaryResponse> GetSummaryAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            var items = await _feedbackRepository.GetByKnowledgeBaseIdAsync(knowledgeBaseId, null, cancellationToken);
            var total = items.Count;
            var helpful = items.Count(x => x.IsHelpful);

            return new FeedbackSummaryResponse
            {
                KnowledgeBaseId = knowledgeBaseId,
                TotalFeedbackCount = total,
                HelpfulCount = helpful,
                UnhelpfulCount = total - helpful,
                HelpfulRate = total > 0 ? Math.Round((double)helpful / total, 4) : 0.0,
                RecentUnhelpful = items
                    .Where(x => !x.IsHelpful)
                    .Take(10)
                    .Select(MapToResponse)
                    .ToList()
            };
        }

        private static FeedbackResponse MapToResponse(AnswerFeedback entity)
        {
            return new FeedbackResponse
            {
                Id = entity.Id,
                KnowledgeBaseId = entity.KnowledgeBaseId,
                ConversationId = entity.ConversationId,
                ChatMessageId = entity.ChatMessageId,
                Question = entity.Question,
                Answer = entity.Answer,
                Sources = string.IsNullOrWhiteSpace(entity.SourcesJson)
                    ? new List<AskSource>()
                    : JsonSerializer.Deserialize<List<AskSource>>(entity.SourcesJson) ?? new List<AskSource>(),
                IsHelpful = entity.IsHelpful,
                Comment = entity.Comment,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
