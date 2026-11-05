using Microsoft.EntityFrameworkCore;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Domain.Entities;
using RagPipeline.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Infrastructure.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly RagDbContext _dbContext;

        public ConversationRepository(RagDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Conversation> CreateAsync(Guid knowledgeBaseId, string title, CancellationToken cancellationToken = default)
        {
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                KnowledgeBaseId = knowledgeBaseId,
                Title = title,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Conversations.AddAsync(conversation, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return conversation;
        }

        public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Conversation>> GetByKnowledgeBaseIdAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Conversations
                .AsNoTracking()
                .Where(x => x.KnowledgeBaseId == knowledgeBaseId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
        {
            await _dbContext.ChatMessages.AddAsync(message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(Guid conversationId, int limit = 10, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatMessages
                .AsNoTracking()
                .Where(x => x.ConversationId == conversationId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ChatMessage>> GetAllMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ChatMessages
                .AsNoTracking()
                .Where(x => x.ConversationId == conversationId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            _dbContext.Conversations.Remove(conversation);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
