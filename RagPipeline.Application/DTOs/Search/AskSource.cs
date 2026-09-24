using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Search
{
    public class AskSource
    {
        public string FileName { get; set; } = string.Empty;

        public Guid ChunkId { get; set; }

        public int ChunkIndex { get; set; }

        public double RerankScore { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
