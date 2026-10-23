using RagPipeline.Application.DTOs.Documents;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;
using Pgvector;

namespace RagPipeline.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ITextExtractionService _textExtractionService;
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;

        public DocumentService(
            IDocumentRepository documentRepository,
            ITextExtractionService textExtractionService,
            IChunkingService chunkingService,
            IEmbeddingService embeddingService)
        {
            _documentRepository = documentRepository;
            _textExtractionService = textExtractionService;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
        }

        public async Task<DocumentResponse> UploadAsync(Guid knowledgeBaseId, Stream stream, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
        {
            var knowledgeBaseExists = await _documentRepository.KnowledgeBaseExistsAsync(knowledgeBaseId, cancellationToken);

            if (!knowledgeBaseExists)
            {
                throw new KeyNotFoundException($"Knowledge base '{knowledgeBaseId}' was not found.");
            }

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

            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunks,cancellationToken);

            if (embeddings.Count != chunks.Count)
            {
                throw new InvalidOperationException("Embedding count does not match chunk count.");
            }

            if (embeddings.Any(x => x.Length != 768))
            {
                throw new InvalidOperationException("Expected 768-dimensional embeddings.");
            }

            var documentId = Guid.NewGuid();

            var document = new Document
            {
                Id = documentId,
                KnowledgeBaseId = knowledgeBaseId,
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
                        Embedding = new Vector(embeddings[i]),
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

        public async Task<List<DocumentResponse>> GetAllAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
        {
            var documents = await _documentRepository.GetAllByKnowledgeBaseIdAsync(knowledgeBaseId, cancellationToken);
            return documents.Select(Map).ToList();
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // We need the tracking entity to remove it
            var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
            if (document is null)
            {
                return false;
            }

            await _documentRepository.DeleteAsync(document, cancellationToken);
            return true;
        }

        private static DocumentResponse Map(Document document)
        {
            return new DocumentResponse
            {
                Id = document.Id,
                KnowledgeBaseId = document.KnowledgeBaseId,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                CreatedAt = document.CreatedAt
            };
        }
    }
}
