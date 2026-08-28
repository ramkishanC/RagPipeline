using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IVectorSearchService _vectorSearchService;

        public SearchController(IVectorSearchService vectorSearchService)
        {
            _vectorSearchService = vectorSearchService;
        }

        [HttpPost("vector")]
        public async Task<ActionResult<VectorSearchResponse>> VectorSearch(
            VectorSearchRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _vectorSearchService.SearchAsync(request.Query,request.TopK,cancellationToken);

            return Ok(result);
        }
    }
}
