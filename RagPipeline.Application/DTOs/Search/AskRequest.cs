using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Search
{
    public class AskRequest
    {
        public Guid KnowledgeBaseId { get; set; }

        public string Query { get; set; } = string.Empty;

        public int TopK { get; set; } = 5;

        public string? Category { get; set; }

        public List<string>? Tags { get; set; }
    }
}
