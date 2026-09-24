using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Infrastructure.Models.Ollama
{
    public class OllamaChatRequest
    {
        public string Model { get; set; } = string.Empty;

        public List<OllamaChatMessage> Messages { get; set; } = [];

        public bool Stream { get; set; }
    }
}
