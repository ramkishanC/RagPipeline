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
        private readonly IAnswerGenerationService _answerGenerationService;

        private readonly double _minimumRerankScore;

        public SearchController(IVectorSearchService vectorSearchService, IKeywordSearchService keywordSearchService, IHybridSearchService hybridSearchService, IRerankedSearchService rerankedSearchService, IAnswerGenerationService answerGenerationService, IConfiguration configuration)
        {
            _vectorSearchService = vectorSearchService;
            _keywordSearchService = keywordSearchService;
            _hybridSearchService = hybridSearchService;
            _rerankedSearchService = rerankedSearchService;
            _answerGenerationService = answerGenerationService;
            _minimumRerankScore = configuration.GetValue<double>("Rag:MinimumRerankScore");
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

        [HttpPost("ask")]
        public async Task<ActionResult<AskResponse>> Ask(AskRequest request, CancellationToken cancellationToken)
        {
            var rerankedResult = await _rerankedSearchService.SearchAsync( request.Query, request.TopK, cancellationToken);

            var relevantResults = rerankedResult.Results
                .Where(x => x.RerankScore >= _minimumRerankScore)
                .ToList();

            if (relevantResults.Count == 0)
            {
                return Ok(new AskResponse
                {
                    Query = request.Query,
                    Answer = "I don't have enough information in the provided documents to answer this question.",
                    Sources = []
                });
            }

            var sources = relevantResults
                .Select((x, index) => new AskSource
                {
                    Citation = $"[{index + 1}]",
                    FileName = x.FileName,
                    ChunkId = x.ChunkId,
                    ChunkIndex = x.ChunkIndex,
                    RerankScore = x.RerankScore,
                    Content = x.Content
                })
                .ToList();

            var context = relevantResults
                .Select((x, index) =>
                    $"SOURCE [{index + 1}]\n{x.Content}")
                .ToList();

            var answer = await _answerGenerationService.GenerateAnswerAsync(request.Query, context, cancellationToken);

            return Ok(new AskResponse
            {
                Query = request.Query,
                Answer = answer,
                Sources = sources
            });
        }
    }
}
