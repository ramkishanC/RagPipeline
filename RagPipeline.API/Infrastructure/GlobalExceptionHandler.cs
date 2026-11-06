using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RagPipeline.API.Infrastructure
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var traceId = httpContext.TraceIdentifier;
            _logger.LogError(exception, "Unhandled exception occurred on {Method} {Path}. TraceId: {TraceId}", httpContext.Request.Method, httpContext.Request.Path, traceId);

            var (statusCode, title) = exception switch
            {
                ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid request parameters."),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request parameters."),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access."),
                TaskCanceledException or OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Request was cancelled or timed out."),
                HttpRequestException => (StatusCodes.Status503ServiceUnavailable, "Downstream AI or reranking service is currently unavailable."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected server error occurred.")
            };

            if (httpContext.Response.HasStarted)
            {
                return true;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Instance = httpContext.Request.Path,
                Detail = _environment.IsDevelopment() ? exception.Message : null
            };
            problemDetails.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
