using RagPipeline.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IConversationService
    {
        Task<Conversation> CreateAsync(Guid knowledgeBaseId, string title, CancellationToken cancellationToken = default);
        Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Conversation>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default);
        Task<Guid> AddMessageAsync(Guid conversationId, string role, string content, string? citationsJson = null, CancellationToken cancellationToken = default);
        Task<List<ChatMessage>> GetRecentMessagesAsync(Guid conversationId, int limit = 10, CancellationToken cancellationToken = default);
        Task<List<ChatMessage>> GetAllMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
