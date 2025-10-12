using System.Collections.Concurrent;
using System.Net;

namespace GarageManagementSystem.API.Middleware
{
    /// <summary>
    /// Rate limiting middleware to prevent abuse
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        
        // Store request counts per IP
        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
        
        // Configuration
        private const int MaxRequestsPerMinute = 60;
        private const int MaxRequestsPerHour = 1000;
        
        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIp(context);
            
            // Skip rate limiting for localhost in development
            if (clientIp == "::1" || clientIp == "127.0.0.1")
            {
                await _next(context);
                return;
            }

            var clientInfo = _clients.GetOrAdd(clientIp, _ => new ClientRequestInfo());

            bool isMinuteExceeded = false;
            bool isHourExceeded = false;

            lock (clientInfo)
            {
                var now = DateTime.Now;

                // Reset counters if time window has passed
                if ((now - clientInfo.MinuteWindowStart).TotalMinutes >= 1)
                {
                    clientInfo.RequestsThisMinute = 0;
                    clientInfo.MinuteWindowStart = now;
                }

                if ((now - clientInfo.HourWindowStart).TotalHours >= 1)
                {
                    clientInfo.RequestsThisHour = 0;
                    clientInfo.HourWindowStart = now;
                }

                // Increment counters
                clientInfo.RequestsThisMinute++;
                clientInfo.RequestsThisHour++;
                clientInfo.LastRequestTime = now;

                // Check limits
                isMinuteExceeded = clientInfo.RequestsThisMinute > MaxRequestsPerMinute;
                isHourExceeded = clientInfo.RequestsThisHour > MaxRequestsPerHour;
            }

            // Handle rate limit outside of lock
            if (isMinuteExceeded)
            {
                _logger.LogWarning("Rate limit exceeded (per minute) for IP: {IP}", clientIp);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Too many requests. Please try again later.",
                    retryAfter = 60
                });
                return;
            }

            if (isHourExceeded)
            {
                _logger.LogWarning("Rate limit exceeded (per hour) for IP: {IP}", clientIp);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = "3600";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Hourly rate limit exceeded. Please try again later.",
                    retryAfter = 3600
                });
                return;
            }

            await _next(context);
        }

        private string GetClientIp(HttpContext context)
        {
            // Check for forwarded IP first (behind proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        // Clean up old client info periodically
        public static void CleanupOldClients()
        {
            var cutoff = DateTime.Now.AddHours(-2);
            var keysToRemove = _clients
                .Where(kvp => kvp.Value.LastRequestTime < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _clients.TryRemove(key, out _);
            }
        }

        private class ClientRequestInfo
        {
            public int RequestsThisMinute { get; set; }
            public int RequestsThisHour { get; set; }
            public DateTime MinuteWindowStart { get; set; } = DateTime.Now;
            public DateTime HourWindowStart { get; set; } = DateTime.Now;
            public DateTime LastRequestTime { get; set; } = DateTime.Now;
        }
    }
}

