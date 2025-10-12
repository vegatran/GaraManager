using System.Net;
using System.Text.Json;

namespace GarageManagementSystem.API.Middleware
{
    /// <summary>
    /// Global error handling middleware
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Success = false,
                Message = GetUserFriendlyMessage(exception),
                TraceId = context.TraceIdentifier
            };

            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Invalid request parameters.";
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "Unauthorized access.";
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = "Resource not found.";
                    break;

                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = exception.Message;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "An internal server error occurred.";
                    break;
            }

            // Include detailed error in development
            if (_env.IsDevelopment())
            {
                errorResponse.Details = exception.ToString();
                errorResponse.StackTrace = exception.StackTrace;
            }

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(json);
        }

        private string GetUserFriendlyMessage(Exception exception)
        {
            // Map common exceptions to user-friendly messages
            return exception switch
            {
                ArgumentNullException => "A required parameter is missing.",
                ArgumentException => "Invalid parameter value provided.",
                UnauthorizedAccessException => "You don't have permission to access this resource.",
                KeyNotFoundException => "The requested resource was not found.",
                InvalidOperationException => exception.Message,
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        private class ErrorResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? Details { get; set; }
            public string? StackTrace { get; set; }
            public string? TraceId { get; set; }
        }
    }
}

