using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.Feedback;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly ICurrentUserService _currentUserService;

        public FeedbackController(
            IFeedbackService feedbackService,
            IKnowledgeBaseService knowledgeBaseService,
            ICurrentUserService currentUserService)
        {
            _feedbackService = feedbackService;
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(FeedbackResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<FeedbackResponse>> Submit(SubmitFeedbackRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return BadRequest("User identity is required.");
            if (request.KnowledgeBaseId == Guid.Empty) return BadRequest("KnowledgeBaseId is required.");

            if (!await _knowledgeBaseService.HasAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
            }

            try
            {
                return Ok(await _feedbackService.SubmitFeedbackAsync(request, cancellationToken));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<FeedbackResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<FeedbackResponse>>> GetAll(
            [FromQuery] Guid knowledgeBaseId,
            [FromQuery] bool? onlyHelpful = null,
            CancellationToken cancellationToken = default)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return BadRequest("User identity is required.");
            if (knowledgeBaseId == Guid.Empty) return BadRequest("KnowledgeBaseId is required.");

            if (!await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
            }

            return Ok(await _feedbackService.GetFeedbackAsync(knowledgeBaseId, onlyHelpful, cancellationToken));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(FeedbackSummaryResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<FeedbackSummaryResponse>> GetSummary(
            [FromQuery] Guid knowledgeBaseId,
            CancellationToken cancellationToken = default)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return BadRequest("User identity is required.");
            if (knowledgeBaseId == Guid.Empty) return BadRequest("KnowledgeBaseId is required.");

            if (!await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
            }

            return Ok(await _feedbackService.GetSummaryAsync(knowledgeBaseId, cancellationToken));
        }
    }
}
