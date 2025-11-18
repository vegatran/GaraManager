using GarageManagementSystem.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GarageManagementSystem.API.Services
{
    /// <summary>
    /// Service for sending real-time notifications via SignalR
    /// </summary>
    public interface INotificationService
    {
        Task NotifyInventoryAlertUpdatedAsync(int alertCount);
        Task NotifyInventoryAlertCreatedAsync(string alertType, int partId, string partName, string message);
        Task NotifyInventoryAlertResolvedAsync(int partId, string partName);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Notify all clients that inventory alert count has been updated
        /// </summary>
        public async Task NotifyInventoryAlertUpdatedAsync(int alertCount)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("InventoryAlertUpdated", new
                {
                    count = alertCount,
                    timestamp = DateTime.UtcNow
                });
                _logger.LogInformation("Sent inventory alert count update: {Count}", alertCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending inventory alert count update");
            }
        }

        /// <summary>
        /// Notify all clients that a new inventory alert has been created
        /// </summary>
        public async Task NotifyInventoryAlertCreatedAsync(string alertType, int partId, string partName, string message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("InventoryAlertCreated", new
                {
                    alertType = alertType,
                    partId = partId,
                    partName = partName,
                    message = message,
                    timestamp = DateTime.UtcNow
                });
                _logger.LogInformation("Sent inventory alert created notification: {AlertType} for part {PartId}", alertType, partId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending inventory alert created notification");
            }
        }

        /// <summary>
        /// Notify all clients that an inventory alert has been resolved
        /// </summary>
        public async Task NotifyInventoryAlertResolvedAsync(int partId, string partName)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("InventoryAlertResolved", new
                {
                    partId = partId,
                    partName = partName,
                    timestamp = DateTime.UtcNow
                });
                _logger.LogInformation("Sent inventory alert resolved notification for part {PartId}", partId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending inventory alert resolved notification");
            }
        }
    }
}

