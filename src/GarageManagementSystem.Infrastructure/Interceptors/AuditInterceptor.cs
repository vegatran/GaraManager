using GarageManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace GarageManagementSystem.Infrastructure.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditFields(DbContext? context)
        {
            if (context == null) return;

            var currentUser = GetCurrentUser();

            var entries = context.ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.Now;
                        entry.Entity.CreatedBy = currentUser;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.Now;
                        entry.Entity.UpdatedBy = currentUser;
                        break;

                    case EntityState.Deleted:
                        // Soft delete instead of hard delete
                        entry.State = EntityState.Modified;
                        entry.Entity.DeletedAt = DateTime.Now;
                        entry.Entity.DeletedBy = currentUser;
                        entry.Entity.IsDeleted = true;
                        break;
                }
            }
        }

        private string? GetCurrentUser()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    // Try to get user ID from claims
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                        return userId;

                    // Fallback to name
                    var userName = httpContext.User.Identity.Name;
                    if (!string.IsNullOrEmpty(userName))
                        return userName;

                    // Fallback to email
                    var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email))
                        return email;
                }
            }
            catch
            {
                // Ignore errors and return null
            }

            return "System"; // Default for system operations
        }
    }
}
