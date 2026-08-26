using RagPipeline.Application.DTOs.Documents;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<DocumentResponse> CreateAsync(string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
        {
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                ContentType = contentType,
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow
            };

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
