using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs.Documents;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<DocumentResponse>> Upload([FromQuery] Guid knowledgeBaseId, IFormFile file, CancellationToken cancellationToken)
        {
            if (knowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (file is null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            var allowedExtensions = new[] { ".pdf", ".txt", ".md" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Only PDF, TXT and Markdown files are supported.");
            }

            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest("File size cannot exceed 10 MB.");
            }

            await using var stream = file.OpenReadStream();

            var document = await _documentService.UploadAsync(knowledgeBaseId,stream,file.FileName,file.ContentType,file.Length,cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }

        [HttpGet] 
        public async Task<ActionResult<List<DocumentResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var documents = await _documentService.GetAllAsync(cancellationToken);
            return Ok(documents);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var document = await _documentService.GetByIdAsync(id, cancellationToken);
            return document is null ? NotFound() : Ok(document);
        }
    }
}
