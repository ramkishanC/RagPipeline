using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.Admin;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IObservabilityService _observabilityService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly ICurrentUserService _currentUserService;

        public AdminController(IObservabilityService observabilityService, IKnowledgeBaseService knowledgeBaseService, ICurrentUserService currentUserService)
        {
            _observabilityService = observabilityService;
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AdminDashboardResponse>> GetDashboard([FromQuery] Guid knowledgeBaseId, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return BadRequest("User identity is required.");
            if (knowledgeBaseId == Guid.Empty) return BadRequest("KnowledgeBaseId is required.");

            if (!await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
            }

            var dashboard = await _observabilityService.GetDashboardAsync(knowledgeBaseId, cancellationToken);
            return Ok(dashboard);
        }

        [HttpGet("queries")]
        [ProducesResponseType(typeof(List<QueryTelemetryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<QueryTelemetryResponse>>> GetRecentQueries([FromQuery] Guid knowledgeBaseId, [FromQuery] bool? onlyNoAnswer = null, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return BadRequest("User identity is required.");
            if (knowledgeBaseId == Guid.Empty) return BadRequest("KnowledgeBaseId is required.");

            if (!await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
            }

            var logs = await _observabilityService.GetRecentQueriesAsync(knowledgeBaseId, onlyNoAnswer, limit, cancellationToken);
            return Ok(logs);
        }
    }
}
