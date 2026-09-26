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
    }
}
