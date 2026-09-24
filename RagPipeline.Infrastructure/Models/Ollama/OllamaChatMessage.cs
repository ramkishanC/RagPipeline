using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Infrastructure.Models.Ollama
{
    public class OllamaChatMessage
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}
