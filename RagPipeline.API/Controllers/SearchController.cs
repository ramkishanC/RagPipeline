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
        private readonly IHybridSearchService _hybridSearchService;
        private readonly IRerankedSearchService _rerankedSearchService;

        public SearchController(IVectorSearchService vectorSearchService, IKeywordSearchService keywordSearchService, IHybridSearchService hybridSearchService, IRerankedSearchService rerankedSearchService)
        {
            _vectorSearchService = vectorSearchService;
            _keywordSearchService = keywordSearchService;
            _hybridSearchService = hybridSearchService;
            _rerankedSearchService = rerankedSearchService;
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

        [HttpPost("hybrid")]
        public async Task<ActionResult<HybridSearchResponse>> HybridSearch(HybridSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _hybridSearchService.SearchAsync(request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }

        [HttpPost("reranked")]
        public async Task<ActionResult<RerankedSearchResponse>> RerankedSearch(HybridSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _rerankedSearchService.SearchAsync(request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }
    }
}
