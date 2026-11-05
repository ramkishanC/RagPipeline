using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.KnowledgeBases;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeBaseController : ControllerBase
    {
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly ICurrentUserService _currentUserService;

        public KnowledgeBaseController(IKnowledgeBaseService knowledgeBaseService, ICurrentUserService currentUserService)
        {
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
        }

        /// <summary>Creates a knowledge base owned by the current user.</summary>
        /// <param name="request">The knowledge base name to create.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpPost]
        [ProducesResponseType(typeof(KnowledgeBaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<KnowledgeBaseResponse>> Create([FromBody] CreateKnowledgeBaseRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Knowledge base name is required.");
            }

            var knowledgeBase = await _knowledgeBaseService.CreateAsync(externalUserId, request.Name, cancellationToken);
            return Ok(knowledgeBase);
        }

        /// <summary>Checks whether the current user owns the requested knowledge base.</summary>
        /// <param name="id">The knowledge base identifier.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpGet("{id:guid}/access")]
        [ProducesResponseType(typeof(KnowledgeBaseAccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<KnowledgeBaseAccessResponse>> CheckAccess(Guid id, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (!await _knowledgeBaseService.HasAccessAsync(id, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
            }

            return Ok(new KnowledgeBaseAccessResponse
            {
                KnowledgeBaseId = id,
                ExternalUserId = externalUserId,
                Access = true
            });
        }

        [HttpGet]
        public async Task<ActionResult<List<KnowledgeBaseResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return Unauthorized("A valid user token is required.");
            }

            var knowledgeBases = await _knowledgeBaseService.GetAllAsync(externalUserId, cancellationToken);
            return Ok(knowledgeBases);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<KnowledgeBaseResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return Unauthorized("A valid user token is required.");
            }

            var knowledgeBase = await _knowledgeBaseService.GetByIdAsync(id, externalUserId, cancellationToken);

            if (knowledgeBase is null)
            {
                return NotFound();
            }

            return Ok(knowledgeBase);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<KnowledgeBaseResponse>> Update(Guid id, [FromBody] UpdateKnowledgeBaseRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return Unauthorized("A valid user token is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Knowledge base name is required.");
            }

            var knowledgeBase = await _knowledgeBaseService.UpdateAsync(id, externalUserId, request, cancellationToken);

            if (knowledgeBase is null)
            {
                return NotFound();
            }

            return Ok(knowledgeBase);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return Unauthorized("A valid user token is required.");
            }

            var deleted = await _knowledgeBaseService.DeleteAsync(id, externalUserId, cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
