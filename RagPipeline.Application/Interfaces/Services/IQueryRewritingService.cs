using RagPipeline.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IQueryRewritingService
    {
        Task<string> RewriteQueryAsync(string query, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default);
    }
}
