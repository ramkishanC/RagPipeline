using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Evaluation
{
    public class RagEvaluationRequest
    {
        public Guid KnowledgeBaseId { get; set; }
        public int TopK { get; set; } = 5;
        public double? MinimumRerankScoreOverride { get; set; }
        public List<RagEvaluationTestCase> TestCases { get; set; } = new();
    }

    public class RagEvaluationTestCase
    {
        public string Question { get; set; } = string.Empty;
        public List<string> ExpectedAnswerKeywords { get; set; } = new();
        public string? ExpectedFileName { get; set; }
        public int? ExpectedChunkIndex { get; set; }
        public bool ExpectNoAnswer { get; set; }
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class RagEvaluationCaseResult
    {
        public string Question { get; set; } = string.Empty;
        public string GeneratedAnswer { get; set; } = string.Empty;
        public bool RetrievalHit { get; set; }
        public int? MatchedRank { get; set; }
        public double ReciprocalRank { get; set; }
        public double AnswerKeywordRecall { get; set; }
        public bool AnswerPassed { get; set; }
        public bool CitationPassed { get; set; }
        public bool NoAnswerPassed { get; set; }
        public List<string> RetrievedSources { get; set; } = new();
    }

    public class RagEvaluationSummaryResponse
    {
        public Guid KnowledgeBaseId { get; set; }
        public int TotalTestCases { get; set; }
        public double RetrievalHitRate { get; set; }
        public double MeanReciprocalRank { get; set; }
        public double AnswerAccuracyRate { get; set; }
        public double CitationAccuracyRate { get; set; }
        public double NoAnswerAccuracyRate { get; set; }
        public List<RagEvaluationCaseResult> Results { get; set; } = new();
    }
}
