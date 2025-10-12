using Microsoft.AspNetCore.Mvc.Filters;
using GarageManagementSystem.Core.Services;

namespace GarageManagementSystem.API.Attributes
{
    /// <summary>
    /// Attribute to automatically log API actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuditAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _entityName;
        private readonly string _action;

        public AuditAttribute(string entityName, string action)
        {
            _entityName = entityName;
            _action = action;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var auditService = context.HttpContext.RequestServices.GetRequiredService<IAuditLogService>();
            
            // Get user info (would come from authentication)
            var userId = context.HttpContext.User?.Identity?.Name ?? "Anonymous";

            // Get entity ID from route parameters if available
            int? entityId = null;
            if (context.ActionArguments.ContainsKey("id"))
            {
                if (int.TryParse(context.ActionArguments["id"]?.ToString(), out var id))
                {
                    entityId = id;
                }
            }

            // Execute action
            var executedContext = await next();

            // Log audit after successful execution
            if (executedContext.Exception == null)
            {
                var details = $"Method: {context.HttpContext.Request.Method}, " +
                             $"Path: {context.HttpContext.Request.Path}, " +
                             $"IP: {context.HttpContext.Connection.RemoteIpAddress}";

                await auditService.LogActionAsync(_entityName, entityId, _action, userId, details);
            }
        }
    }
}

