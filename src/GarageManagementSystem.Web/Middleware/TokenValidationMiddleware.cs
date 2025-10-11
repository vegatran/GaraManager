using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GarageManagementSystem.Web.Middleware
{
    /// <summary>
    /// Middleware để validate token expiry và redirect nếu token hết hạn
    /// </summary>
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidationMiddleware> _logger;

        public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Chỉ check cho authenticated users và không phải API calls
            if (context.User.Identity?.IsAuthenticated == true && 
                !context.Request.Path.StartsWithSegments("/api") &&
                !context.Request.Path.StartsWithSegments("/Home/Login") &&
                !context.Request.Path.StartsWithSegments("/signin-oidc") &&
                !context.Request.Path.StartsWithSegments("/signout-callback-oidc"))
            {
                // Check if access token exists and is expired
                var accessToken = await context.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(accessToken);
                        
                        // Check if token is expired
                        if (jsonToken.ValidTo < DateTime.UtcNow)
                        {
                            _logger.LogWarning("🔒 Access token expired for user: {User}, redirecting to login", 
                                context.User.Identity.Name);
                            
                            // Clear authentication cookies
                            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            
                            // Redirect to login
                            context.Response.Redirect("/Home/Login");
                            return;
                        }
                        
                        _logger.LogDebug("✅ Access token valid for user: {User}, expires: {Expiry}", 
                            context.User.Identity.Name, jsonToken.ValidTo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error validating access token for user: {User}", 
                            context.User.Identity.Name);
                        
                        // If token is invalid, clear authentication
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        context.Response.Redirect("/Home/Login");
                        return;
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ No access token found for authenticated user: {User}", 
                        context.User.Identity.Name);
                    
                    // If no access token, clear authentication
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.Redirect("/Home/Login");
                    return;
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method để đăng ký middleware
    /// </summary>
    public static class TokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidationMiddleware>();
        }
    }
}
