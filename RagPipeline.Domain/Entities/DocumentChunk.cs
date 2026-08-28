using System;
using System.Collections.Generic;
using System.Text;
using Pgvector;

namespace RagPipeline.Domain.Entities
{
    public class DocumentChunk
    {
        public Guid Id { get; set; }

        public Guid DocumentId { get; set; }

        public string Content { get; set; } = string.Empty;

        public int ChunkIndex { get; set; }

        public int TokenCount { get; set; }

        public Vector? Embedding { get; set; }

        public DateTime CreatedAt { get; set; }

        public Document Document { get; set; } = null!;
    }
}
