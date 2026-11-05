using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Domain.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; }

        public Guid KnowledgeBaseId { get; set; }

        public string Title { get; set; } = "New Conversation";

        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public KnowledgeBase KnowledgeBase { get; set; } = null!;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
