using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs.KnowledgeBases;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeBaseController : ControllerBase
    {
        private readonly IKnowledgeBaseService _knowledgeBaseService;

        public KnowledgeBaseController(IKnowledgeBaseService knowledgeBaseService)
        {
            _knowledgeBaseService = knowledgeBaseService;
        }

        [HttpPost]
        public async Task<ActionResult<KnowledgeBaseResponse>> Create(CreateKnowledgeBaseRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ExternalUserId))
            {
                return BadRequest("ExternalUserId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Knowledge base name is required.");
            }

            var knowledgeBase = await _knowledgeBaseService.CreateAsync(request, cancellationToken);
            return Ok(knowledgeBase);
        }

        [HttpGet("{id:guid}/access")]
        public async Task<IActionResult> CheckAccess(Guid id, [FromHeader(Name = "X-User-Id")] string externalUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (!await _knowledgeBaseService.HasAccessAsync(id, externalUserId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new
                    {
                        message = "You do not have access to this knowledge base."
                    });
            }

            return Ok(new
            {
                knowledgeBaseId = id,
                externalUserId,
                access = true
            });
        }
    }
}
