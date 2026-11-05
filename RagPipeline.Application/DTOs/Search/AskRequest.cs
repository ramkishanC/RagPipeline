using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Search
{
    public class AskRequest
    {
        public Guid KnowledgeBaseId { get; set; }

        public Guid? ConversationId { get; set; }

        public string Query { get; set; } = string.Empty;

        public int TopK { get; set; } = 5;

        public string? Category { get; set; }

        public List<string>? Tags { get; set; }

        public double VectorWeight { get; set; } = 1.0;

        public double KeywordWeight { get; set; } = 1.0;

        public int ContextWindowSize { get; set; } = 1;
    }
}
