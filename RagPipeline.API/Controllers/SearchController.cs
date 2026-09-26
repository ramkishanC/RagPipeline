using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Services;
using System.Text.RegularExpressions;

namespace RagPipeline.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IVectorSearchService _vectorSearchService;
        private readonly IKeywordSearchService _keywordSearchService;
        private readonly IHybridSearchService _hybridSearchService;
        private readonly IRerankedSearchService _rerankedSearchService;
        private readonly IAnswerGenerationService _answerGenerationService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly ICurrentUserService _currentUserService;

        private readonly double _minimumRerankScore;

        public SearchController(IVectorSearchService vectorSearchService, IKeywordSearchService keywordSearchService, IHybridSearchService hybridSearchService, IRerankedSearchService rerankedSearchService, IAnswerGenerationService answerGenerationService, IConfiguration configuration, IKnowledgeBaseService knowledgeBaseService, ICurrentUserService currentUserService)
        {
            _vectorSearchService = vectorSearchService;
            _keywordSearchService = keywordSearchService;
            _hybridSearchService = hybridSearchService;
            _rerankedSearchService = rerankedSearchService;
            _answerGenerationService = answerGenerationService;
            _minimumRerankScore = configuration.GetValue<double>("Rag:MinimumRerankScore");
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
        }

        /// <summary>Searches vectors within a knowledge base owned by the current user.</summary>
        /// <param name="request">The knowledge base, query, and result limit.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpPost("vector")]
        [ProducesResponseType(typeof(VectorSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<VectorSearchResponse>> VectorSearch(VectorSearchRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (request.KnowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
            }

            var result = await _vectorSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }

        /// <summary>Searches document text within a knowledge base owned by the current user.</summary>
        /// <param name="request">The knowledge base, query, and result limit.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpPost("keyword")]
        [ProducesResponseType(typeof(KeywordSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<KeywordSearchResponse>> KeywordSearch(KeywordSearchRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (request.KnowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
            }

            var result = await _keywordSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }

        /// <summary>Combines vector and keyword results for a knowledge base owned by the current user.</summary>
        /// <param name="request">The knowledge base, query, and result limit.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpPost("hybrid")]
        [ProducesResponseType(typeof(HybridSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HybridSearchResponse>> HybridSearch(HybridSearchRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (request.KnowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
            }

            var result = await _hybridSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }

        /// <summary>Reranks hybrid results for a knowledge base owned by the current user.</summary>
        /// <param name="request">The knowledge base, query, and result limit.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpPost("reranked")]
        [ProducesResponseType(typeof(RerankedSearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RerankedSearchResponse>> RerankedSearch(HybridSearchRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (request.KnowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
            }

            var result = await _rerankedSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, cancellationToken);

            return Ok(result);
        }

        /// <summary>Generates a cited answer using documents from a knowledge base owned by the current user.</summary>
        /// <param name="request">The knowledge base, question, and retrieval limit.</param>
        /// <param name="cancellationToken">The request cancellation token.</param>
        [HttpPost("ask")]
        [ProducesResponseType(typeof(AskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AskResponse>> Ask(AskRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();

            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                return BadRequest("X-User-Id header is required.");
            }

            if (request.KnowledgeBaseId == Guid.Empty)
            {
                return BadRequest("KnowledgeBaseId is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
            }

            var rerankedResult = await _rerankedSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, cancellationToken);

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
                    $"[{index + 1}] {x.Content}")
                .ToList();

            var answer = await _answerGenerationService.GenerateAnswerAsync(request.Query, context, cancellationToken);

            if (!HasValidCitations(answer, sources.Count))
            {
                return Ok(new AskResponse
                {
                    Query = request.Query,
                    Answer = "I could not verify the generated answer against the retrieved sources.",
                    Sources = sources
                });
            }

            return Ok(new AskResponse
            {
                Query = request.Query,
                Answer = answer,
                Sources = sources
            });
        }

        private static bool HasValidCitations(string answer, int sourceCount)
        {
            if (sourceCount <= 0)
            {
                return true;
            }

            var matches = Regex.Matches(answer, @"\[(\d+)\]");

            if (matches.Count == 0)
            {
                return false;
            }

            foreach (Match match in matches)
            {
                if (!int.TryParse(match.Groups[1].Value, out var citationNumber))
                {
                    return false;
                }

                if (citationNumber < 1 || citationNumber > sourceCount)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> HasKnowledgeBaseAccessAsync(Guid knowledgeBaseId, string externalUserId, CancellationToken cancellationToken)
        {
            return await _knowledgeBaseService.HasAccessAsync(knowledgeBaseId, externalUserId, cancellationToken);
        }

        private ObjectResult ForbiddenKnowledgeBase()
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse { Message = "You do not have access to this knowledge base." });
        }
    }
}
