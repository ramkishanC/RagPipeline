using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepository;

        public ConversationService(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<Conversation> CreateAsync(Guid knowledgeBaseId, string title, CancellationToken cancellationToken = default)
        {
            return await _conversationRepository.CreateAsync(knowledgeBaseId, title, cancellationToken);
        }

        public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _conversationRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<List<Conversation>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            return await _conversationRepository.GetByKnowledgeBaseIdAsync(knowledgeBaseId, cancellationToken);
        }

        public async Task AddMessageAsync(Guid conversationId, string role, string content, string? citationsJson = null, CancellationToken cancellationToken = default)
        {
            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = role,
                Content = content,
                CitationsJson = citationsJson,
                CreatedAt = DateTime.UtcNow
            };

            await _conversationRepository.AddMessageAsync(message, cancellationToken);
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(Guid conversationId, int limit = 10, CancellationToken cancellationToken = default)
        {
            return await _conversationRepository.GetRecentMessagesAsync(conversationId, limit, cancellationToken);
        }

        public async Task<List<ChatMessage>> GetAllMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            return await _conversationRepository.GetAllMessagesAsync(conversationId, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var conversation = await _conversationRepository.GetByIdAsync(id, cancellationToken);
            if (conversation == null) return false;

            await _conversationRepository.DeleteAsync(conversation, cancellationToken);
            return true;
        }
    }
}
