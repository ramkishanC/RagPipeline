using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Conversations
{
    public class ChatMessageResponse
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? CitationsJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
