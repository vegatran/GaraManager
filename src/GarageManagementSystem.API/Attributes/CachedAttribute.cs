using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GarageManagementSystem.Core.Services;
using System.Text;

namespace GarageManagementSystem.API.Attributes
{
    /// <summary>
    /// Attribute to enable caching for API endpoints
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _durationMinutes;

        public CachedAttribute(int durationMinutes = 5)
        {
            _durationMinutes = durationMinutes;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            
            // Generate cache key from request
            var cacheKey = GenerateCacheKey(context.HttpContext.Request);

            // Try to get from cache
            var cachedResponse = cacheService.Get<object>(cacheKey);
            if (cachedResponse != null)
            {
                context.Result = new OkObjectResult(cachedResponse);
                return;
            }

            // Execute action
            var executedContext = await next();

            // Cache the result if successful
            if (executedContext.Result is OkObjectResult okResult)
            {
                cacheService.Set(cacheKey, okResult.Value, TimeSpan.FromMinutes(_durationMinutes));
            }
        }

        private string GenerateCacheKey(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");

            // Include query parameters
            foreach (var query in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{query.Key}={query.Value}");
            }

            return keyBuilder.ToString();
        }
    }
}

