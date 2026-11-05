using RagPipeline.Application.DTOs.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IRagEvaluationService
    {
        Task<RagEvaluationSummaryResponse> EvaluateAsync(RagEvaluationRequest request, double defaultMinimumRerankScore, CancellationToken cancellationToken = default);
    }
}
