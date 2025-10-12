using System.Diagnostics;

namespace GarageManagementSystem.API.Middleware
{
    /// <summary>
    /// Middleware to log all API requests
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var clientIp = GetClientIp(context);

            try
            {
                _logger.LogInformation(
                    "Request started: {Method} {Path} from {IP}",
                    requestMethod,
                    requestPath,
                    clientIp);

                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                    requestMethod,
                    requestPath,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "Request failed: {Method} {Path} - Duration: {Duration}ms - Error: {Message}",
                    requestMethod,
                    requestPath,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                throw;
            }
        }

        private string GetClientIp(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}

