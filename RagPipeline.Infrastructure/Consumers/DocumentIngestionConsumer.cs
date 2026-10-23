using MassTransit;
using Microsoft.Extensions.Logging;
using RagPipeline.Application.DTOs.Events;
using RagPipeline.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Infrastructure.Consumers
{
    public class DocumentIngestionConsumer : IConsumer<DocumentUploadedEvent>
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentIngestionConsumer> _logger;

        public DocumentIngestionConsumer(IDocumentService documentService, ILogger<DocumentIngestionConsumer> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DocumentUploadedEvent> context)
        {
            _logger.LogInformation("Received DocumentUploadedEvent for Document ID: {DocumentId}", context.Message.DocumentId);

            // MassTransit handles retries, so we just call the service layer
            await _documentService.ProcessDocumentAsync(context.Message.DocumentId, context.CancellationToken);

            _logger.LogInformation("Successfully processed Document ID: {DocumentId}", context.Message.DocumentId);
        }
    }
}
