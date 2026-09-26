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
        private readonly IKnowledgeBaseService _knowledgeBaseService;

        public DocumentsController(IDocumentService documentService, IKnowledgeBaseService knowledgeBaseService)
        {
            _documentService = documentService;
            _knowledgeBaseService = knowledgeBaseService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<DocumentResponse>> Upload([FromQuery] Guid knowledgeBaseId, [FromHeader(Name = "X-User-Id")] string externalUserId, IFormFile file, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (knowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(knowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
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
        public async Task<ActionResult<List<DocumentResponse>>> GetAll([FromQuery] Guid knowledgeBaseId, [FromHeader(Name = "X-User-Id")] string externalUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (knowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(knowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
            }

            var documents = await _documentService.GetAllAsync(knowledgeBaseId, cancellationToken);
            return Ok(documents);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DocumentResponse>> GetById(Guid id, [FromHeader(Name = "X-User-Id")] string externalUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            var document = await _documentService.GetByIdAsync(id, cancellationToken);

            if (document is null)
            {
                return NotFound();
            }

            if (!await HasKnowledgeBaseAccessAsync(document.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have access to this document." });
            }

            return Ok(document);
        }

        private async Task<bool> HasKnowledgeBaseAccessAsync(Guid knowledgeBaseId, string externalUserId, CancellationToken cancellationToken)
        {
            return await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken);
        }

        private ObjectResult ForbiddenKnowledgeBase()
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have access to this knowledge base." });
        }
    }
}
