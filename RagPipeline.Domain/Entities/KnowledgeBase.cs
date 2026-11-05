using System;
using System.Collections.Generic;

namespace RagPipeline.Domain.Entities
{
    public class KnowledgeBase
    {
        public Guid Id { get; set; }

        public Guid RagUserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public RagUser RagUser { get; set; } = null!;

        public ICollection<Document> Documents { get; set; } = new List<Document>();

        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
