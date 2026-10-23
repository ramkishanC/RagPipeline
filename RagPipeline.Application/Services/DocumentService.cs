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

        public async Task<DocumentResponse> UploadAsync(Guid knowledgeBaseId, Stream stream, string fileName, string contentType, long fileSize, string? category = null, List<string>? tags = null, CancellationToken cancellationToken = default)
        {
            var knowledgeBaseExists = await _documentRepository.KnowledgeBaseExistsAsync(knowledgeBaseId, cancellationToken);
            if (!knowledgeBaseExists)
            {
                throw new KeyNotFoundException($"Knowledge base '{knowledgeBaseId}' was not found.");
            }

            // 1. Save file locally for the background worker
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadsFolder);
            var localFilePath = Path.Combine(uploadsFolder, $"{Guid.NewGuid()}_{fileName}");

            await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream, cancellationToken);

            // 2. Save Document Record as 'Pending'
            var document = new Document
            {
                Id = Guid.NewGuid(),
                KnowledgeBaseId = knowledgeBaseId,
                FileName = fileName,
                ContentType = contentType,
                FileSize = fileSize,
                LocalFilePath = localFilePath,
                Status = Domain.Enums.DocumentStatus.Pending,
                Category = category,
                Tags = tags ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document, cancellationToken);

            return Map(document);
        }

        public async Task ProcessDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
            if (document == null || document.Status != Domain.Enums.DocumentStatus.Pending) return;

            try
            {
                document.Status = Domain.Enums.DocumentStatus.Processing;
                await _documentRepository.UpdateAsync(document, cancellationToken);

                await using var stream = new FileStream(document.LocalFilePath, FileMode.Open, FileAccess.Read);
                var text = await _textExtractionService.ExtractAsync(stream, document.FileName, cancellationToken);

                if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("No text could be extracted from the document.");

                var chunks = _chunkingService.Chunk(text);
                if (chunks.Count == 0) throw new InvalidOperationException("No chunks could be created from the document.");

                var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunks, cancellationToken);

                for (var i = 0; i < chunks.Count; i++)
                {
                    document.Chunks.Add(new DocumentChunk
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = document.Id,
                        Content = chunks[i],
                        ChunkIndex = i,
                        TokenCount = Math.Max(1, (int)Math.Ceiling(chunks[i].Length / 4.0)),
                        Embedding = new Vector(embeddings[i]),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                document.Status = Domain.Enums.DocumentStatus.Completed;
            }
            catch (Exception ex)
            {
                document.Status = Domain.Enums.DocumentStatus.Failed;
                document.ErrorMessage = ex.Message;
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(document.LocalFilePath))
                {
                    File.Delete(document.LocalFilePath);
                }

                await _documentRepository.UpdateAsync(document, cancellationToken);
            }
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
                CreatedAt = document.CreatedAt,
                Status = document.Status.ToString(),
                ErrorMessage = document.ErrorMessage,
                Category = document.Category,
                Tags = document.Tags
            };
        }
    }
}
