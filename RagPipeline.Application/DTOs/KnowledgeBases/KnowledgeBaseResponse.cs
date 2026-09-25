using System;

namespace RagPipeline.Application.DTOs.KnowledgeBases
{
    public class KnowledgeBaseResponse
    {
        public Guid Id { get; set; }

        public string ExternalUserId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
