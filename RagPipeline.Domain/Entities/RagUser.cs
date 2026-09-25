using System;
using System.Collections.Generic;

namespace RagPipeline.Domain.Entities
{
    public class RagUser
    {
        public Guid Id { get; set; }

        public string ExternalUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<KnowledgeBase> KnowledgeBases { get; set; } = new List<KnowledgeBase>();
    }
}
