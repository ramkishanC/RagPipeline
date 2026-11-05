using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Events
{
    public class DocumentUploadedEvent
    {
        public Guid DocumentId { get; set; }
    }
}
