using RagPipeline.Application.DTOs.Documents;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ITextExtractionService _textExtractionService;
        private readonly IChunkingService _chunkingService;

        public DocumentService(
            IDocumentRepository documentRepository,
            ITextExtractionService textExtractionService,
            IChunkingService chunkingService)
        {
            _documentRepository = documentRepository;
            _textExtractionService = textExtractionService;
            _chunkingService = chunkingService;
        }

        public async Task<DocumentResponse> UploadAsync(Stream stream, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
        {
            var text = await _textExtractionService.ExtractAsync(stream,fileName,cancellationToken);

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException(
                    "No text could be extracted from the document.");
            }

            var chunks = _chunkingService.Chunk(text);

            if (chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "No chunks could be created from the document.");
            }

            var documentId = Guid.NewGuid();

            var document = new Document
            {
                Id = documentId,
                FileName = fileName,
                ContentType = contentType,
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow
            };

            for (var i = 0; i < chunks.Count; i++)
            {
                var content = chunks[i];

                document.Chunks.Add(
                    new DocumentChunk
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        Content = content,
                        ChunkIndex = i,
                        TokenCount = Math.Max(1, (int)Math.Ceiling(content.Length / 4.0)),
                        CreatedAt = DateTime.UtcNow
                    });
            }

            await _documentRepository.AddAsync(document, cancellationToken);
            return Map(document);
        }

        public async Task<DocumentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
            return document is null ? null : Map(document);
        }

        public async Task<List<DocumentResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var documents = await _documentRepository.GetAllAsync(cancellationToken);
            return documents.Select(Map).ToList();
        }

        private static DocumentResponse Map(Document document)
        {
            return new DocumentResponse
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                CreatedAt = document.CreatedAt
            };
        }
    }
}
