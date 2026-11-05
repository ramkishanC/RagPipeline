using RagPipeline.Application.DTOs.Evaluation;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RagPipeline.Application.Services
{
    public class RagEvaluationService : IRagEvaluationService
    {
        private const string NoContextFallback = "I don't have enough information in the provided documents to answer this question.";
        private const string LlmAbstentionPhrase = "The information is not available in the provided context.";

        private readonly IRerankedSearchService _rerankedSearchService;
        private readonly IAnswerGenerationService _answerGenerationService;

        public RagEvaluationService(
            IRerankedSearchService rerankedSearchService,
            IAnswerGenerationService answerGenerationService)
        {
            _rerankedSearchService = rerankedSearchService;
            _answerGenerationService = answerGenerationService;
        }

        public async Task<RagEvaluationSummaryResponse> EvaluateAsync(
            RagEvaluationRequest request,
            double defaultMinimumRerankScore,
            CancellationToken cancellationToken = default)
        {
            var minScore = request.MinimumRerankScoreOverride ?? defaultMinimumRerankScore;
            var caseResults = new List<RagEvaluationCaseResult>();

            foreach (var testCase in request.TestCases)
            {
                if (string.IsNullOrWhiteSpace(testCase.Question))
                {
                    continue;
                }

                var reranked = await _rerankedSearchService.SearchAsync(
                    request.KnowledgeBaseId,
                    testCase.Question,
                    request.TopK,
                    testCase.Category,
                    testCase.Tags,
                    cancellationToken);

                var relevantResults = reranked.Results
                    .Where(x => x.RerankScore >= minScore)
                    .ToList();

                // 1. Handle Negative / Out-of-Scope Test Cases
                if (testCase.ExpectNoAnswer)
                {
                    string generatedAnswer;
                    if (relevantResults.Count == 0)
                    {
                        generatedAnswer = NoContextFallback;
                    }
                    else
                    {
                        var negContext = relevantResults.Select((x, i) => $"[{i + 1}] {x.Content}").ToList();
                        generatedAnswer = await _answerGenerationService.GenerateAnswerAsync(
                            testCase.Question,
                            negContext,
                            Array.Empty<ChatMessage>(),
                            cancellationToken);
                    }

                    var abstained = relevantResults.Count == 0 ||
                                    generatedAnswer.Trim().Equals(LlmAbstentionPhrase, StringComparison.OrdinalIgnoreCase);

                    caseResults.Add(new RagEvaluationCaseResult
                    {
                        Question = testCase.Question,
                        GeneratedAnswer = generatedAnswer,
                        RetrievalHit = abstained,
                        ReciprocalRank = 0,
                        AnswerKeywordRecall = 1.0,
                        AnswerPassed = abstained,
                        CitationPassed = abstained,
                        NoAnswerPassed = abstained,
                        RetrievedSources = relevantResults.Select(x => $"{x.FileName} (chunk {x.ChunkIndex})").ToList()
                    });

                    continue;
                }

                // 2. Evaluate Retrieval & Reranking Rank for Positive Test Cases
                int? matchedRank = null;
                for (var i = 0; i < relevantResults.Count; i++)
                {
                    var item = relevantResults[i];
                    var fileMatches = string.IsNullOrWhiteSpace(testCase.ExpectedFileName) ||
                                      item.FileName.Equals(testCase.ExpectedFileName, StringComparison.OrdinalIgnoreCase);
                    var chunkMatches = !testCase.ExpectedChunkIndex.HasValue ||
                                       item.ChunkIndex == testCase.ExpectedChunkIndex.Value;

                    if (fileMatches && chunkMatches)
                    {
                        matchedRank = i + 1;
                        break;
                    }
                }

                var retrievalHit = matchedRank.HasValue;
                var reciprocalRank = matchedRank.HasValue ? 1.0 / matchedRank.Value : 0.0;

                if (relevantResults.Count == 0)
                {
                    caseResults.Add(new RagEvaluationCaseResult
                    {
                        Question = testCase.Question,
                        GeneratedAnswer = NoContextFallback,
                        RetrievalHit = false,
                        MatchedRank = null,
                        ReciprocalRank = 0.0,
                        AnswerKeywordRecall = 0.0,
                        AnswerPassed = false,
                        CitationPassed = false,
                        NoAnswerPassed = true
                    });
                    continue;
                }

                // 3. Generate Answer
                var context = relevantResults.Select((x, index) => $"[{index + 1}] {x.Content}").ToList();
                var answer = await _answerGenerationService.GenerateAnswerAsync(
                    testCase.Question,
                    context,
                    Array.Empty<ChatMessage>(),
                    cancellationToken);

                // 4. Evaluate Answer Keyword Recall
                double keywordRecall = 1.0;
                if (testCase.ExpectedAnswerKeywords.Count > 0)
                {
                    var matchedKeywords = testCase.ExpectedAnswerKeywords.Count(kw =>
                        answer.Contains(kw, StringComparison.OrdinalIgnoreCase));
                    keywordRecall = (double)matchedKeywords / testCase.ExpectedAnswerKeywords.Count;
                }

                var answerPassed = keywordRecall >= 1.0;

                // 5. Evaluate Citation Correctness
                var citationPassed = EvaluateCitations(answer, relevantResults.Count, matchedRank);

                caseResults.Add(new RagEvaluationCaseResult
                {
                    Question = testCase.Question,
                    GeneratedAnswer = answer,
                    RetrievalHit = retrievalHit,
                    MatchedRank = matchedRank,
                    ReciprocalRank = Math.Round(reciprocalRank, 4),
                    AnswerKeywordRecall = Math.Round(keywordRecall, 4),
                    AnswerPassed = answerPassed,
                    CitationPassed = citationPassed,
                    NoAnswerPassed = true,
                    RetrievedSources = relevantResults.Select((x, i) => $"[{i + 1}] {x.FileName} (chunk {x.ChunkIndex})").ToList()
                });
            }

            var positiveCases = caseResults.Where((r, idx) => !request.TestCases[idx].ExpectNoAnswer).ToList();
            var negativeCases = caseResults.Where((r, idx) => request.TestCases[idx].ExpectNoAnswer).ToList();

            return new RagEvaluationSummaryResponse
            {
                KnowledgeBaseId = request.KnowledgeBaseId,
                TotalTestCases = caseResults.Count,
                RetrievalHitRate = positiveCases.Count > 0
                    ? Math.Round(positiveCases.Count(x => x.RetrievalHit) / (double)positiveCases.Count, 4)
                    : 0.0,
                MeanReciprocalRank = positiveCases.Count > 0
                    ? Math.Round(positiveCases.Average(x => x.ReciprocalRank), 4)
                    : 0.0,
                AnswerAccuracyRate = positiveCases.Count > 0
                    ? Math.Round(positiveCases.Count(x => x.AnswerPassed) / (double)positiveCases.Count, 4)
                    : 0.0,
                CitationAccuracyRate = positiveCases.Count > 0
                    ? Math.Round(positiveCases.Count(x => x.CitationPassed) / (double)positiveCases.Count, 4)
                    : 0.0,
                NoAnswerAccuracyRate = negativeCases.Count > 0
                    ? Math.Round(negativeCases.Count(x => x.NoAnswerPassed) / (double)negativeCases.Count, 4)
                    : 1.0,
                Results = caseResults
            };
        }

        private static bool EvaluateCitations(string answer, int sourceCount, int? expectedRank)
        {
            var matches = Regex.Matches(answer, @"\[(\d+)\]");
            if (matches.Count == 0)
            {
                return false;
            }

            var citedNumbers = new HashSet<int>();
            foreach (Match match in matches)
            {
                if (!int.TryParse(match.Groups[1].Value, out var num) || num < 1 || num > sourceCount)
                {
                    return false;
                }
                citedNumbers.Add(num);
            }

            if (expectedRank.HasValue && !citedNumbers.Contains(expectedRank.Value))
            {
                return false;
            }

            return true;
        }
    }
}
