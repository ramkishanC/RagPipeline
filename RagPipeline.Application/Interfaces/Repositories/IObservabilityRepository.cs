using RagPipeline.Application.DTOs.Admin;
using RagPipeline.Domain.Entities;

namespace RagPipeline.Application.Interfaces.Repositories
{
    public interface IObservabilityRepository
    {
        Task AddTelemetryAsync(RagQueryTelemetry telemetry, CancellationToken cancellationToken = default);
        Task<AdminDashboardResponse> GetDashboardMetricsAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default);
        Task<List<RagQueryTelemetry>> GetRecentTelemetriesAsync(Guid knowledgeBaseId, bool? onlyNoAnswer = null, int limit = 50, CancellationToken cancellationToken = default);
    }
}
