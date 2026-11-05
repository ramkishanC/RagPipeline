using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.Conversations;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Application.Services;

namespace RagPipeline.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly ICurrentUserService _currentUserService;

        public ConversationsController(IConversationService conversationService, IKnowledgeBaseService knowledgeBaseService, ICurrentUserService currentUserService)
        {
            _conversationService = conversationService;
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ConversationResponse>>> GetAll([FromQuery] Guid knowledgeBaseId, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return BadRequest("X-User-Id header is required.");
            if (knowledgeBaseId == Guid.Empty) return BadRequest("KnowledgeBaseId is required.");

            if (!await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken))
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "Access denied." });

            var conversations = await _conversationService.GetByKnowledgeBaseIdAsync(knowledgeBaseId, cancellationToken);

            return Ok(conversations.Select(c => new ConversationResponse
            {
                Id = c.Id,
                KnowledgeBaseId = c.KnowledgeBaseId,
                Title = c.Title,
                CreatedAt = c.CreatedAt
            }).ToList());
        }

        [HttpGet("{id:guid}/messages")]
        public async Task<ActionResult<List<ChatMessageResponse>>> GetMessages(Guid id, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return BadRequest("X-User-Id header is required.");

            var conversation = await _conversationService.GetByIdAsync(id, cancellationToken);
            if (conversation == null) return NotFound();

            if (!await _knowledgeBaseService.HasAccessAsync(conversation.KnowledgeBaseId, externalUserId, cancellationToken))
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "Access denied." });

            var messages = await _conversationService.GetAllMessagesAsync(id, cancellationToken);

            return Ok(messages.Select(m => new ChatMessageResponse
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CitationsJson = m.CitationsJson,
                CreatedAt = m.CreatedAt
            }).ToList());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId)) return Unauthorized();

            var conversation = await _conversationService.GetByIdAsync(id, cancellationToken);
            if (conversation == null) return NotFound();

            if (!await _knowledgeBaseService.HasAccessAsync(conversation.KnowledgeBaseId, externalUserId, cancellationToken))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." });

            var deleted = await _conversationService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}