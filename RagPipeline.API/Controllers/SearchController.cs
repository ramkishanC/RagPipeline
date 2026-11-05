using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs;
using RagPipeline.Application.DTOs.Search;
using RagPipeline.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;
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
        private readonly IConversationService _conversationService;
        private readonly IQueryRewritingService _queryRewritingService;

        private readonly double _minimumRerankScore;

        public SearchController(IVectorSearchService vectorSearchService, IKeywordSearchService keywordSearchService, IHybridSearchService hybridSearchService, IRerankedSearchService rerankedSearchService, IAnswerGenerationService answerGenerationService, IConfiguration configuration, IKnowledgeBaseService knowledgeBaseService, ICurrentUserService currentUserService, IConversationService conversationService, IQueryRewritingService queryRewritingService)
        {
            _vectorSearchService = vectorSearchService;
            _keywordSearchService = keywordSearchService;
            _hybridSearchService = hybridSearchService;
            _rerankedSearchService = rerankedSearchService;
            _answerGenerationService = answerGenerationService;
            _minimumRerankScore = configuration.GetValue<double>("Rag:MinimumRerankScore");
            _knowledgeBaseService = knowledgeBaseService;
            _currentUserService = currentUserService;
            _conversationService = conversationService;
            _queryRewritingService = queryRewritingService;
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

            var result = await _vectorSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, request.Category, request.Tags, cancellationToken);
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

            var result = await _keywordSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, request.Category, request.Tags, cancellationToken);
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

            var result = await _hybridSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, request.Category, request.Tags, cancellationToken);
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

            var result = await _rerankedSearchService.SearchAsync(request.KnowledgeBaseId, request.Query, request.TopK, request.Category, request.Tags, cancellationToken);

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

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Query is required.");
            }

            if (!await HasKnowledgeBaseAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                return ForbiddenKnowledgeBase();
            }

            // 1. Resolve or Create Conversation
            Guid currentConversationId;
            if (request.ConversationId.HasValue && request.ConversationId.Value != Guid.Empty)
            {
                var conversation = await _conversationService.GetByIdAsync(request.ConversationId.Value, cancellationToken);
                if (conversation == null || conversation.KnowledgeBaseId != request.KnowledgeBaseId)
                {
                    return BadRequest("Invalid ConversationId for this Knowledge Base.");
                }
                currentConversationId = conversation.Id;
            }
            else
            {
                var title = request.Query.Length > 50 ? request.Query[..50] + "..." : request.Query;
                var newConv = await _conversationService.CreateAsync(request.KnowledgeBaseId, title, cancellationToken);
                currentConversationId = newConv.Id;
            }

            // 2. Fetch history BEFORE saving the current user message (last 6 messages = 3 full turns)
            var history = await _conversationService.GetRecentMessagesAsync(currentConversationId, 6, cancellationToken);

            // 3. Save current user query to DB via Service Layer
            await _conversationService.AddMessageAsync(currentConversationId, "user", request.Query, null, cancellationToken);

            // 4. Rewrite query into a standalone search query using conversation history
            var standaloneQuery = await _queryRewritingService.RewriteQueryAsync(request.Query, history, cancellationToken);

            // 5. Execute RAG Retrieval using the standalone rewritten query
            var rerankedResult = await _rerankedSearchService.SearchAsync(request.KnowledgeBaseId, standaloneQuery, request.TopK, request.Category, request.Tags, cancellationToken);

            var relevantResults = rerankedResult.Results
                .Where(x => x.RerankScore >= _minimumRerankScore)
                .ToList();

            // 6. Handle "No Context Found" Gracefully
            if (relevantResults.Count == 0)
            {
                var noContextAnswer = "I don't have enough information in the provided documents to answer this question.";

                // Save AI fallback response via Service Layer
                var noContextMsgId = await _conversationService.AddMessageAsync(currentConversationId, "assistant", noContextAnswer, null, cancellationToken);

                return Ok(new AskResponse
                {
                    ConversationId = currentConversationId,
                    MessageId = noContextMsgId,
                    Query = request.Query,
                    RewrittenQuery = standaloneQuery,
                    Answer = noContextAnswer,
                    Sources = []
                });
            }

            // 7. Format Sources & Context
            var sources = relevantResults
                .Select((x, index) => new AskSource
                {
                    Citation = $"[{index + 1}]",
                    DocumentId = x.DocumentId,
                    FileName = x.FileName,
                    ChunkId = x.ChunkId,
                    ChunkIndex = x.ChunkIndex,
                    RerankScore = x.RerankScore,
                    Content = x.Content
                })
                .ToList();

            var context = relevantResults
                .Select((x, index) => $"[{index + 1}] {x.Content}")
                .ToList();

            // 8. Generate Answer via Ollama (passing history)
            var answer = await _answerGenerationService.GenerateAnswerAsync(request.Query, context, history, cancellationToken);

            // 9. Validate & Mark Cited Sources
            if (!HasValidCitations(answer, sources.Count))
            {
                var fallbackAnswer = "I could not verify the generated answer against the retrieved sources.";
                var fallbackMsgId = await _conversationService.AddMessageAsync(currentConversationId, "assistant", fallbackAnswer, JsonSerializer.Serialize(sources), cancellationToken);

                return Ok(new AskResponse
                {
                    ConversationId = currentConversationId,
                    MessageId = fallbackMsgId,
                    Query = request.Query,
                    RewrittenQuery = standaloneQuery,
                    Answer = fallbackAnswer,
                    Sources = sources
                });
            }

            MarkCitedSources(answer, sources);

            // 10. Save AI Response to DB with serialized Citations via Service Layer
            var assistantMessageId = await _conversationService.AddMessageAsync(currentConversationId, "assistant", answer, JsonSerializer.Serialize(sources), cancellationToken);

            return Ok(new AskResponse
            {
                ConversationId = currentConversationId,
                MessageId = assistantMessageId,
                Query = request.Query,
                RewrittenQuery = standaloneQuery,
                Answer = answer,
                Sources = sources
            });
        }

        /// <summary>Streams a cited answer using Server-Sent Events (SSE).</summary>
        [HttpPost("ask/stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task AskStream(AskRequest request, CancellationToken cancellationToken)
        {
            var externalUserId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(externalUserId))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("X-User-Id header is required.");
                return;
            }

            if (request.KnowledgeBaseId == Guid.Empty)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("KnowledgeBaseId is required.", cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Query is required.", cancellationToken);
                return;
            }

            if (!await HasKnowledgeBaseAccessAsync(request.KnowledgeBaseId, externalUserId, cancellationToken))
            {
                Response.StatusCode = 403;
                await Response.WriteAsJsonAsync(new ApiErrorResponse { Message = "You do not have access to this knowledge base." }, cancellationToken);
                return;
            }

            // 1. Resolve/Create Conversation BEFORE starting SSE stream so invalid IDs return 400
            Guid currentConversationId;
            if (request.ConversationId.HasValue && request.ConversationId.Value != Guid.Empty)
            {
                var conversation = await _conversationService.GetByIdAsync(request.ConversationId.Value, cancellationToken);
                if (conversation == null || conversation.KnowledgeBaseId != request.KnowledgeBaseId)
                {
                    Response.StatusCode = 400;
                    await Response.WriteAsync("Invalid ConversationId for this Knowledge Base.", cancellationToken);
                    return;
                }
                currentConversationId = conversation.Id;
            }
            else
            {
                var title = request.Query.Length > 50 ? request.Query[..50] + "..." : request.Query;
                var newConv = await _conversationService.CreateAsync(request.KnowledgeBaseId, title, cancellationToken);
                currentConversationId = newConv.Id;
            }

            // 2. Setup SSE Headers
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            // 3. History, User Message Saving & Query Rewriting
            var history = await _conversationService.GetRecentMessagesAsync(currentConversationId, 6, cancellationToken);
            await _conversationService.AddMessageAsync(currentConversationId, "user", request.Query, null, cancellationToken);
            var standaloneQuery = await _queryRewritingService.RewriteQueryAsync(request.Query, history, cancellationToken);

            // Send initial metadata payload (including the rewritten query)
            var metaPayload = JsonSerializer.Serialize(new
            {
                type = "meta",
                conversationId = currentConversationId,
                rewrittenQuery = standaloneQuery
            });
            await Response.WriteAsync($"data: {metaPayload}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            // 4. RAG Retrieval using the standalone rewritten query
            var rerankedResult = await _rerankedSearchService.SearchAsync(request.KnowledgeBaseId, standaloneQuery, request.TopK, request.Category, request.Tags, cancellationToken);
            var relevantResults = rerankedResult.Results.Where(x => x.RerankScore >= _minimumRerankScore).ToList();

            if (relevantResults.Count == 0)
            {
                var noContextAnswer = "I don't have enough information in the provided documents to answer this question.";
                var noContextMessageId = await _conversationService.AddMessageAsync(currentConversationId, "assistant", noContextAnswer, null, cancellationToken);

                var payload = JsonSerializer.Serialize(new { type = "chunk", messageId = noContextMessageId, text = noContextAnswer });
                await Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
                await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
                return;
            }

            // 5. Context Prep
            var sources = relevantResults.Select((x, index) => new AskSource
            {
                Citation = $"[{index + 1}]",
                DocumentId = x.DocumentId,
                FileName = x.FileName,
                ChunkId = x.ChunkId,
                ChunkIndex = x.ChunkIndex,
                RerankScore = x.RerankScore,
                Content = x.Content
            }).ToList();

            var context = relevantResults.Select((x, index) => $"[{index + 1}] {x.Content}").ToList();

            // 6. Execute Streaming
            var stream = _answerGenerationService.GenerateStreamedAnswerAsync(request.Query, context, history, cancellationToken);
            var fullAnswer = new StringBuilder();

            await foreach (var chunk in stream)
            {
                fullAnswer.Append(chunk);

                var chunkPayload = JsonSerializer.Serialize(new { type = "chunk", text = chunk });
                await Response.WriteAsync($"data: {chunkPayload}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            // 7. Mark Cited Sources, Emit Sources Event & Save Assistant Message to DB
            var finalAnswerString = fullAnswer.ToString();
            var citationsValid = HasValidCitations(finalAnswerString, sources.Count);
            MarkCitedSources(finalAnswerString, sources);

            var assistantMessageId = await _conversationService.AddMessageAsync(currentConversationId, "assistant", finalAnswerString, JsonSerializer.Serialize(sources), cancellationToken);

            var sourcesPayload = JsonSerializer.Serialize(new
            {
                type = "sources",
                messageId = assistantMessageId,
                citationsValid = citationsValid,
                sources = sources
            });
            await Response.WriteAsync($"data: {sourcesPayload}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            // Signal stream completion
            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }


        private static void MarkCitedSources(string answer, List<AskSource> sources)
        {
            var citedIndices = Regex.Matches(answer, @"\[(\d+)\]")
                .Select(m => int.TryParse(m.Groups[1].Value, out var n) ? n : -1)
                .Where(n => n >= 1 && n <= sources.Count)
                .ToHashSet();

            for (var i = 0; i < sources.Count; i++)
            {
                sources[i].IsCited = citedIndices.Contains(i + 1);
            }
        }

        private static bool HasValidCitations(string answer, int sourceCount)
        {
            if (sourceCount <= 0)
            {
                return true;
            }

            if (answer.Trim().Equals("The information is not available in the provided context.", StringComparison.OrdinalIgnoreCase))
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
