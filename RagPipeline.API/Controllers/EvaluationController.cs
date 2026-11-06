using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.Evaluation;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationController : ControllerBase
    {
        private readonly IRagEvaluationService _evaluationService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly ICurrentUserService _currentUserService;
        private readonly double _defaultMinimumRerankScore;

        public EvaluationController(
            IRagEvaluationService evaluationService,
            IKnowledgeBaseService knowledgeBaseService,
            ICurrentUserService currentUserService,
            IConfiguration configuration)
        {
            _evaluationService = evaluationService;
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
            _defaultMinimumRerankScore = configuration.GetValue<double>("Rag:MinimumRerankScore");
        }

        /// <summary>Runs a benchmark dataset against a knowledge base and computes RAG quality metrics.</summary>
        [HttpPost("run")]
        [ProducesResponseType(typeof(RagEvaluationSummaryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RagEvaluationSummaryResponse>> RunEvaluation(
            RagEvaluationRequest request,
            CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("User identity is required.");
            }

            if (request.KnowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (request.TestCases == null || request.TestCases.Count == 0)
            {
                return BadRequest("At least one test case is required.");
            }

            if (!await _knowledgeBaseService.HasAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
                {
                    Message = "You do not have access to this knowledge base."
                });
            }

            var summary = await _evaluationService.EvaluateAsync(
                request,
                _defaultMinimumRerankScore,
                cancellationToken);

            return Ok(summary);
        }
    }
}
