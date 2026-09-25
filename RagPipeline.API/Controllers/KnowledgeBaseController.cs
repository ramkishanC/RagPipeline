using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RagPipeline.Application.DTOs.KnowledgeBases;
using RagPipeline.Domain.Entities;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KnowledgeBaseController : ControllerBase
    {
        private readonly RagDbContext _dbContext;

        public KnowledgeBaseController(RagDbContext dbContext)
        {
            _dbContext = dbContext;
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

            var user = await _dbContext.RagUsers.FirstOrDefaultAsync(x => x.ExternalUserId == request.ExternalUserId, cancellationToken);

            if (user is null)
            {
                user = new RagUser
                {
                    Id = Guid.NewGuid(),
                    ExternalUserId = request.ExternalUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.RagUsers.AddAsync(user, cancellationToken);
            }

            var knowledgeBase = new KnowledgeBase
            {
                Id = Guid.NewGuid(),
                RagUserId = user.Id,
                Name = request.Name.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.KnowledgeBases.AddAsync(knowledgeBase, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new KnowledgeBaseResponse
            {
                Id = knowledgeBase.Id,
                ExternalUserId = user.ExternalUserId,
                Name = knowledgeBase.Name,
                CreatedAt = knowledgeBase.CreatedAt
            });
        }
    }
}
