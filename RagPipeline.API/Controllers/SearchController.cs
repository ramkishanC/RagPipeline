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
        private readonly IKeywordSearchService _keywordSearchService;

        public SearchController(
            IVectorSearchService vectorSearchService,
            IKeywordSearchService keywordSearchService)
        {
            _vectorSearchService = vectorSearchService;
            _keywordSearchService = keywordSearchService;
        }

        [HttpPost("vector")]
        public async Task<ActionResult<VectorSearchResponse>> VectorSearch(VectorSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _vectorSearchService.SearchAsync(request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }

        [HttpPost("keyword")]
        public async Task<ActionResult<KeywordSearchResponse>> KeywordSearch(KeywordSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _keywordSearchService.SearchAsync(request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }
    }
}
