using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace GarageManagementSystem.Web.Middleware
{
    /// <summary>
    /// Middleware để xử lý token timeout và chuyển hướng đến trang đăng nhập
    /// </summary>
    public class TokenTimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenTimeoutMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public TokenTimeoutMiddleware(RequestDelegate next, ILogger<TokenTimeoutMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Chỉ xử lý 401 Unauthorized responses cho non-API requests
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";
                
                // Skip middleware cho API calls - để JavaScript handle
                if (path.StartsWith("/api/") || path.StartsWith("/employeemanagement/") || 
                    path.StartsWith("/customermanagement/") || path.StartsWith("/vehiclemanagement/") ||
                    path.StartsWith("/appointmentmanagement/") || path.StartsWith("/servicemanagement/") ||
                    path.StartsWith("/ordermanagement/") || path.StartsWith("/setup/"))
                {
                    _logger.LogDebug("Skipping middleware for API/Controller path: {Path}", path);
                    return;
                }

                _logger.LogWarning("401 Unauthorized detected for path: {Path}", path);
                
                // Check if response has started
                if (!context.Response.HasStarted)
                {
                    try
                    {
                        // Clear authentication cookies
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        await context.SignOutAsync("oidc");
                        
                        // Redirect về IdentityServer end session endpoint
                        var identityServerAuthority = _configuration["IdentityServer:Authority"];
                        var endSessionUrl = $"{identityServerAuthority}/connect/endsession";
                        context.Response.Redirect(endSessionUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during sign out and redirect for path: {Path}", path);
                    }
                }
                else
                {
                    _logger.LogWarning("Response has already started, cannot modify headers for redirect on path: {Path}", path);
                }
            }
        }
    }

    /// <summary>
    /// Extension method để đăng ký middleware
    /// </summary>
    public static class TokenTimeoutMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenTimeoutHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenTimeoutMiddleware>();
        }
    }
}
