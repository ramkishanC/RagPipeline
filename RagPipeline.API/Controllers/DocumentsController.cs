using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.Documents;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly ICurrentUserService _currentUserService;

        public DocumentsController(IDocumentService documentService, IKnowledgeBaseService knowledgeBaseService, ICurrentUserService currentUserService)
        {
            _documentService = documentService;
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
        }

        /// <summary>Uploads and indexes a document in a knowledge base owned by the current user.</summary>
        /// <param name="knowledgeBaseId">The destination knowledge base identifier.</param>
        /// <param name="file">The PDF, text, or Markdown file to upload.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<DocumentResponse>> Upload([FromQuery] Guid knowledgeBaseId, IFormFile file, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

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

        /// <summary>Lists documents in a knowledge base owned by the current user.</summary>
        /// <param name="knowledgeBaseId">The knowledge base identifier to list documents from.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<DocumentResponse>>> GetAll([FromQuery] Guid knowledgeBaseId, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

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

        /// <summary>Gets a document when it belongs to a knowledge base owned by the current user.</summary>
        /// <param name="id">The document identifier.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

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
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this document." });
            }

            return Ok(document);
        }

        private async Task<bool> HasKnowledgeBaseAccessAsync(Guid knowledgeBaseId, string externalUserId, CancellationToken cancellationToken)
        {
            return await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken);
        }

        private ObjectResult ForbiddenKnowledgeBase()
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
        }
    }
}
