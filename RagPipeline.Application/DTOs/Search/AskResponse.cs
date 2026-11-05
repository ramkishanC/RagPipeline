using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Search
{
    public class AskResponse
    {
        public Guid ConversationId { get; set; }

        public string Query { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public IReadOnlyList<AskSource> Sources { get; set; } = [];
    }
}
