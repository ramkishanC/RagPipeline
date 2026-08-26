using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    }
}
