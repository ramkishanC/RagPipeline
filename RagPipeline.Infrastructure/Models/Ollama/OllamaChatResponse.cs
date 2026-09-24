using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Infrastructure.Models.Ollama
{
    public class OllamaChatResponse
    {
        public string Model { get; set; } = string.Empty;

        public OllamaChatMessage Message { get; set; } = new();

        public bool Done { get; set; }
    }
}
