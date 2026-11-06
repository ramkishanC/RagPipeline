using Microsoft.Extensions.Diagnostics.HealthChecks;
using RagPipeline.Infrastructure.Persistence;

namespace RagPipeline.API.Infrastructure
{
    public class RagInfrastructureHealthCheck : IHealthCheck
    {
        private readonly RagDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RagInfrastructureHealthCheck(RagDbContext dbContext, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>();
            var isDegraded = false;

            try
            {
                var dbConnected = await _dbContext.Database.CanConnectAsync(cancellationToken);
                data["postgres"] = dbConnected ? "Healthy" : "Unhealthy";
                if (!dbConnected)
                {
                    return HealthCheckResult.Unhealthy("PostgreSQL database is unreachable.", data: data);
                }
            }
            catch (Exception ex)
            {
                data["postgres"] = $"Unreachable ({ex.GetType().Name})";
                return HealthCheckResult.Unhealthy("PostgreSQL database is unreachable.", data: data);
            }

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var ollamaBaseUrl = _configuration["Ollama:BaseUrl"];
            if (string.IsNullOrWhiteSpace(ollamaBaseUrl))
            {
                data["ollama"] = "Not configured";
                isDegraded = true;
            }
            else
            {
                try
                {
                    var ollamaResponse = await client.GetAsync(new Uri(new Uri(ollamaBaseUrl), "api/tags"), cancellationToken);
                    data["ollama"] = ollamaResponse.IsSuccessStatusCode ? "Healthy" : $"Degraded ({(int)ollamaResponse.StatusCode})";
                    if (!ollamaResponse.IsSuccessStatusCode) isDegraded = true;
                }
                catch (Exception ex)
                {
                    data["ollama"] = $"Unreachable ({ex.GetType().Name})";
                    isDegraded = true;
                }
            }

            var rerankerBaseUrl = _configuration["Reranker:BaseUrl"];
            if (string.IsNullOrWhiteSpace(rerankerBaseUrl))
            {
                data["reranker"] = "Not configured";
                isDegraded = true;
            }
            else
            {
                try
                {
                    var rerankerResponse = await client.GetAsync(new Uri(rerankerBaseUrl), cancellationToken);
                    data["reranker"] = rerankerResponse.IsSuccessStatusCode ? "Healthy" : $"Degraded ({(int)rerankerResponse.StatusCode})";
                    if (!rerankerResponse.IsSuccessStatusCode) isDegraded = true;
                }
                catch (Exception ex)
                {
                    data["reranker"] = $"Unreachable ({ex.GetType().Name})";
                    isDegraded = true;
                }
            }

            return isDegraded ? HealthCheckResult.Degraded("One or more downstream AI services are degraded.", data: data) : HealthCheckResult.Healthy("All RAG pipeline subsystems are healthy.", data);
        }
    }
}
