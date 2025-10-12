using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Services;

namespace GarageManagementSystem.API.Services
{
    /// <summary>
    /// Background service for scheduled tasks
    /// </summary>
    public class BackgroundJobService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundJobService> _logger;
        private Timer? _maintenanceReminderTimer;
        private Timer? _insuranceReminderTimer;
        private Timer? _rateLimitCleanupTimer;
        private Timer? _cacheCleanupTimer;

        public BackgroundJobService(IServiceProvider serviceProvider, ILogger<BackgroundJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Job Service started");

            // Run daily maintenance reminders at 9 AM
            ScheduleDailyTask(() => SendMaintenanceReminders(), 9, 0, ref _maintenanceReminderTimer);

            // Run insurance expiry reminders at 10 AM
            ScheduleDailyTask(() => SendInsuranceExpiryReminders(), 10, 0, ref _insuranceReminderTimer);

            // Cleanup rate limit cache every hour
            _rateLimitCleanupTimer = new Timer(
                _ => CleanupRateLimitCache(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromHours(1));

            // Cleanup application cache every 2 hours
            _cacheCleanupTimer = new Timer(
                _ => CleanupApplicationCache(),
                null,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(2));

            return Task.CompletedTask;
        }

        private void ScheduleDailyTask(Action task, int hour, int minute, ref Timer? timer)
        {
            var now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

            if (now > scheduledTime)
            {
                // If time has passed today, schedule for tomorrow
                scheduledTime = scheduledTime.AddDays(1);
            }

            var timeUntilFirst = scheduledTime - now;

            timer = new Timer(
                _ => task(),
                null,
                timeUntilFirst,
                TimeSpan.FromDays(1));

            _logger.LogInformation("Scheduled daily task for {Time}", scheduledTime);
        }

        private async void SendMaintenanceReminders()
        {
            try
            {
                _logger.LogInformation("Starting maintenance reminders job");

                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var vehicles = await unitOfWork.Vehicles.GetAllAsync();
                var serviceOrders = await unitOfWork.ServiceOrders.GetAllAsync();

                var cutoffDate = DateTime.Now.AddMonths(-6);
                var remindersCount = 0;

                foreach (var vehicle in vehicles)
                {
                    var lastService = serviceOrders
                        .Where(o => o.VehicleId == vehicle.Id && o.CompletedDate.HasValue)
                        .OrderByDescending(o => o.CompletedDate)
                        .FirstOrDefault();

                    if (lastService == null || lastService.CompletedDate < cutoffDate)
                    {
                        // TODO: Send actual notification (SMS/Email)
                        _logger.LogInformation(
                            "Maintenance reminder needed for vehicle {VehiclePlate} (Customer: {CustomerId})",
                            vehicle.LicensePlate,
                            vehicle.CustomerId);
                        remindersCount++;
                    }
                }

                _logger.LogInformation("Maintenance reminders job completed. Sent {Count} reminders", remindersCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in maintenance reminders job");
            }
        }

        private async void SendInsuranceExpiryReminders()
        {
            try
            {
                _logger.LogInformation("Starting insurance expiry reminders job");

                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var vehicles = await unitOfWork.Vehicles.GetAllAsync();
                var expiryDate = DateTime.Now.AddDays(30);

                var expiringInsurance = vehicles
                    .Where(v => v.HasInsurance &&
                               v.InsuranceEndDate.HasValue &&
                               v.InsuranceEndDate <= expiryDate &&
                               v.InsuranceEndDate >= DateTime.Now)
                    .ToList();

                foreach (var vehicle in expiringInsurance)
                {
                    // TODO: Send actual notification
                    _logger.LogInformation(
                        "Insurance expiry reminder for vehicle {VehiclePlate} - Expires: {ExpiryDate}",
                        vehicle.LicensePlate,
                        vehicle.InsuranceEndDate);
                }

                _logger.LogInformation("Insurance expiry reminders completed. Sent {Count} reminders", expiringInsurance.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in insurance expiry reminders job");
            }
        }

        private void CleanupRateLimitCache()
        {
            try
            {
                _logger.LogInformation("Starting rate limit cache cleanup");
                Middleware.RateLimitingMiddleware.CleanupOldClients();
                _logger.LogInformation("Rate limit cache cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limit cache cleanup");
            }
        }

        private void CleanupApplicationCache()
        {
            try
            {
                _logger.LogInformation("Starting application cache cleanup");
                
                using var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                // Remove old reports cache
                cacheService.RemoveByPrefix("reports:");
                
                _logger.LogInformation("Application cache cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in application cache cleanup");
            }
        }

        public override void Dispose()
        {
            _maintenanceReminderTimer?.Dispose();
            _insuranceReminderTimer?.Dispose();
            _rateLimitCleanupTimer?.Dispose();
            _cacheCleanupTimer?.Dispose();
            base.Dispose();
        }
    }
}

