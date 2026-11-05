using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; set; }

        public Guid ConversationId { get; set; }

        /// <summary>
        /// "user" or "assistant"
        /// </summary>
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// A JSON serialized list of sources/citations used by the assistant.
        /// Will be null for "user" messages.
        /// </summary>
        public string? CitationsJson { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Property
        public Conversation Conversation { get; set; } = null!;
    }
}
