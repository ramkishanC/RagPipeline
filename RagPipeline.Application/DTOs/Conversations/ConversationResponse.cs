using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Conversations
{
    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public Guid KnowledgeBaseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
