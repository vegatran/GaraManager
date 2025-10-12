using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GarageManagementSystem.Core.Services
{
    /// <summary>
    /// Service for audit logging
    /// </summary>
    public interface IAuditLogService
    {
        Task LogActionAsync(string entityName, int? entityId, string action, string? userId, string? details = null);
        Task LogLoginAsync(string userId, bool success, string? ipAddress = null);
        Task LogDataChangeAsync(string entityName, int entityId, string action, object? oldData, object? newData, string? userId);
        Task<IEnumerable<AuditLog>> GetLogsAsync(DateTime? from = null, DateTime? to = null, string? entityName = null, string? userId = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IUnitOfWork unitOfWork, ILogger<AuditLogService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task LogActionAsync(string entityName, int? entityId, string action, string? userId, string? details = null)
        {
            try
            {
                var log = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    UserId = userId,
                    Details = details,
                    Timestamp = DateTime.Now
                };

                await _unitOfWork.Repository<AuditLog>().AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for {EntityName}:{EntityId}", entityName, entityId);
                // Don't throw - audit logging should not break the application
            }
        }

        public async Task LogLoginAsync(string userId, bool success, string? ipAddress = null)
        {
            try
            {
                var log = new AuditLog
                {
                    EntityName = "Authentication",
                    Action = success ? "Login" : "LoginFailed",
                    UserId = userId,
                    Details = $"IP: {ipAddress ?? "Unknown"}",
                    Timestamp = DateTime.Now
                };

                await _unitOfWork.Repository<AuditLog>().AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authentication attempt for user {UserId}", userId);
            }
        }

        public async Task LogDataChangeAsync(string entityName, int entityId, string action, object? oldData, object? newData, string? userId)
        {
            try
            {
                var changes = new System.Text.StringBuilder();
                
                if (oldData != null && newData != null)
                {
                    // Simple change tracking - in production use more sophisticated comparison
                    changes.AppendLine($"Old: {System.Text.Json.JsonSerializer.Serialize(oldData)}");
                    changes.AppendLine($"New: {System.Text.Json.JsonSerializer.Serialize(newData)}");
                }
                else if (newData != null)
                {
                    changes.AppendLine($"Created: {System.Text.Json.JsonSerializer.Serialize(newData)}");
                }
                else if (oldData != null)
                {
                    changes.AppendLine($"Deleted: {System.Text.Json.JsonSerializer.Serialize(oldData)}");
                }

                var log = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    UserId = userId,
                    Details = changes.ToString(),
                    Timestamp = DateTime.Now
                };

                await _unitOfWork.Repository<AuditLog>().AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log data change for {EntityName}:{EntityId}", entityName, entityId);
            }
        }

        public async Task<IEnumerable<AuditLog>> GetLogsAsync(DateTime? from = null, DateTime? to = null, string? entityName = null, string? userId = null)
        {
            var logs = await _unitOfWork.Repository<AuditLog>().GetAllAsync();
            
            var query = logs.AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value);

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(l => l.EntityName == entityName);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            return query.OrderByDescending(l => l.Timestamp).ToList();
        }
    }
}

