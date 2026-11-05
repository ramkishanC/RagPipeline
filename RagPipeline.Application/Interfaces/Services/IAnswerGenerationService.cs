using RagPipeline.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IAnswerGenerationService
    {
        Task<string> GenerateAnswerAsync(string query, IReadOnlyList<string> context, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default);
    }
}
