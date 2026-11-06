using RagPipeline.Application.DTOs.Admin;

namespace RagPipeline.Application.Interfaces.Services
{
    public interface IObservabilityService
    {
        Task RecordQueryTelemetryAsync(RecordQueryTelemetryRequest request, CancellationToken cancellationToken = default);
        Task<AdminDashboardResponse> GetDashboardAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default);
        Task<List<QueryTelemetryResponse>> GetRecentQueriesAsync(Guid knowledgeBaseId, bool? onlyNoAnswer = null, int limit = 50, CancellationToken cancellationToken = default);
    }
}
