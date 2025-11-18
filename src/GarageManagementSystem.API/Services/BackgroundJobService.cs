using GarageManagementSystem.Core.Entities;
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
        private Timer? _inventoryAlertsCheckTimer;
        private readonly SemaphoreSlim _inventoryAlertsCheckLock = new SemaphoreSlim(1, 1); // ✅ FIX: Prevent concurrent execution

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

            // Check inventory alerts every 15 minutes
            _inventoryAlertsCheckTimer = new Timer(
                _ => CheckInventoryAlerts(),
                null,
                TimeSpan.Zero, // Start immediately
                TimeSpan.FromMinutes(15));

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

        /// <summary>
        /// Check inventory alerts and create/update alert records
        /// </summary>
        private async void CheckInventoryAlerts()
        {
            // ✅ FIX: Prevent concurrent execution (if previous job is still running)
            if (!await _inventoryAlertsCheckLock.WaitAsync(0))
            {
                _logger.LogWarning("Inventory alerts check job is already running, skipping this execution");
                return;
            }

            try
            {
                _logger.LogInformation("Starting inventory alerts check job");

                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // ✅ FIX: Filter only active parts to avoid creating alerts for inactive parts
                var allParts = await unitOfWork.Parts.GetAllAsync();
                var parts = allParts.Where(p => p.IsActive).ToList();
                
                var existingAlerts = await unitOfWork.Repository<InventoryAlert>()
                    .GetAllAsync();
                var existingAlertsDict = existingAlerts
                    .Where(a => !a.IsResolved)
                    .GroupBy(a => new { a.PartId, a.AlertType })
                    .ToDictionary(g => g.Key, g => g.First());

                var newAlertsCount = 0;
                var resolvedAlertsCount = 0;

                // Check Low Stock and Out of Stock
                foreach (var part in parts)
                {
                    if (part.QuantityInStock == 0)
                    {
                        // Out of Stock
                        var key = new { PartId = part.Id, AlertType = "OutOfStock" };
                        var lowStockKey = new { PartId = part.Id, AlertType = "LowStock" };
                        
                        // ✅ FIX: Resolve LowStock alert if exists (OutOfStock takes priority)
                        if (existingAlertsDict.ContainsKey(lowStockKey))
                        {
                            var lowStockAlert = existingAlertsDict[lowStockKey];
                            lowStockAlert.IsResolved = true;
                            lowStockAlert.ResolvedDate = DateTime.UtcNow;
                            await unitOfWork.Repository<InventoryAlert>().UpdateAsync(lowStockAlert);
                            resolvedAlertsCount++;
                            
                            await notificationService.NotifyInventoryAlertResolvedAsync(part.Id, part.PartName ?? "");
                        }
                        
                        if (!existingAlertsDict.ContainsKey(key))
                        {
                            var alert = new InventoryAlert
                            {
                                PartId = part.Id,
                                AlertType = "OutOfStock",
                                Severity = "Critical",
                                Message = $"Phụ tùng {part.PartName ?? "N/A"} ({part.PartNumber ?? "N/A"}) đã hết hàng",
                                CurrentQuantity = 0,
                                MinimumQuantity = part.MinimumStock,
                                AlertDate = DateTime.UtcNow,
                                IsResolved = false
                            };
                            await unitOfWork.Repository<InventoryAlert>().AddAsync(alert);
                            newAlertsCount++;

                            // Send notification
                            await notificationService.NotifyInventoryAlertCreatedAsync(
                                "OutOfStock", part.Id, part.PartName ?? "", alert.Message);
                        }
                    }
                    else if (part.QuantityInStock <= part.MinimumStock && part.MinimumStock > 0)
                    {
                        // Low Stock
                        var key = new { PartId = part.Id, AlertType = "LowStock" };
                        var severity = part.QuantityInStock <= part.MinimumStock * 0.5m ? "High" : "Medium";
                        
                        if (!existingAlertsDict.ContainsKey(key))
                        {
                            var alert = new InventoryAlert
                            {
                                PartId = part.Id,
                                AlertType = "LowStock",
                                Severity = severity,
                                Message = $"Phụ tùng {part.PartName ?? "N/A"} ({part.PartNumber ?? "N/A"}) sắp hết hàng. Tồn kho: {part.QuantityInStock}, Tối thiểu: {part.MinimumStock}",
                                CurrentQuantity = part.QuantityInStock,
                                MinimumQuantity = part.MinimumStock,
                                ReorderQuantity = (part.ReorderLevel ?? part.MinimumStock * 2) - part.QuantityInStock,
                                AlertDate = DateTime.UtcNow,
                                IsResolved = false
                            };
                            await unitOfWork.Repository<InventoryAlert>().AddAsync(alert);
                            newAlertsCount++;

                            // Send notification
                            await notificationService.NotifyInventoryAlertCreatedAsync(
                                "LowStock", part.Id, part.PartName ?? "", alert.Message);
                        }
                    }
                    else
                    {
                        // Check if we need to resolve existing alerts
                        var lowStockKey = new { PartId = part.Id, AlertType = "LowStock" };
                        var outOfStockKey = new { PartId = part.Id, AlertType = "OutOfStock" };
                        
                        if (existingAlertsDict.ContainsKey(lowStockKey) && part.QuantityInStock > part.MinimumStock)
                        {
                            var alert = existingAlertsDict[lowStockKey];
                            alert.IsResolved = true;
                            alert.ResolvedDate = DateTime.UtcNow;
                            await unitOfWork.Repository<InventoryAlert>().UpdateAsync(alert);
                            resolvedAlertsCount++;

                            await notificationService.NotifyInventoryAlertResolvedAsync(part.Id, part.PartName ?? "");
                        }
                        
                        if (existingAlertsDict.ContainsKey(outOfStockKey) && part.QuantityInStock > 0)
                        {
                            var alert = existingAlertsDict[outOfStockKey];
                            alert.IsResolved = true;
                            alert.ResolvedDate = DateTime.UtcNow;
                            await unitOfWork.Repository<InventoryAlert>().UpdateAsync(alert);
                            resolvedAlertsCount++;

                            await notificationService.NotifyInventoryAlertResolvedAsync(part.Id, part.PartName ?? "");
                        }
                    }

                    // Check Overstock
                    if (part.ReorderLevel.HasValue && part.QuantityInStock > part.ReorderLevel * 3)
                    {
                        var key = new { PartId = part.Id, AlertType = "Overstock" };
                        if (!existingAlertsDict.ContainsKey(key))
                        {
                            var excess = part.QuantityInStock - (part.ReorderLevel * 3);
                            var alert = new InventoryAlert
                            {
                                PartId = part.Id,
                                AlertType = "Overstock",
                                Severity = "Low",
                                Message = $"Phụ tùng {part.PartName ?? "N/A"} ({part.PartNumber ?? "N/A"}) tồn kho cao. Tồn kho: {part.QuantityInStock}, Tối đa khuyến nghị: {part.ReorderLevel * 3}, Dư: {excess}",
                                CurrentQuantity = part.QuantityInStock,
                                AlertDate = DateTime.UtcNow,
                                IsResolved = false
                            };
                            await unitOfWork.Repository<InventoryAlert>().AddAsync(alert);
                            newAlertsCount++;

                            await notificationService.NotifyInventoryAlertCreatedAsync(
                                "Overstock", part.Id, part.PartName ?? "", alert.Message);
                        }
                    }
                    else
                    {
                        var key = new { PartId = part.Id, AlertType = "Overstock" };
                        if (existingAlertsDict.ContainsKey(key) && 
                            (!part.ReorderLevel.HasValue || part.QuantityInStock <= part.ReorderLevel * 3))
                        {
                            var alert = existingAlertsDict[key];
                            alert.IsResolved = true;
                            alert.ResolvedDate = DateTime.UtcNow;
                            await unitOfWork.Repository<InventoryAlert>().UpdateAsync(alert);
                            resolvedAlertsCount++;

                            await notificationService.NotifyInventoryAlertResolvedAsync(part.Id, part.PartName ?? "");
                        }
                    }
                }

                // Check Expiring Soon
                var batches = await unitOfWork.Repository<PartInventoryBatch>().GetAllAsync();
                var now = DateTime.Now;
                var expiringDate = now.AddDays(30);
                // ✅ FIX: Filter only active batches
                var expiringBatches = batches
                    .Where(b => b.IsActive && // ✅ FIX: Chỉ check active batches
                               b.ExpiryDate.HasValue && 
                               b.ExpiryDate >= now && // ✅ FIX: Chỉ check batches chưa hết hạn
                               b.ExpiryDate <= expiringDate && 
                               b.QuantityRemaining > 0)
                    .ToList();

                // ✅ FIX: Group by PartId and select the batch with earliest expiry date for each part
                var expiringBatchesByPart = expiringBatches
                    .GroupBy(b => b.PartId)
                    .Select(g => g.OrderBy(b => b.ExpiryDate).First()) // Select batch with earliest expiry
                    .ToList();

                foreach (var batch in expiringBatchesByPart)
                {
                    var part = parts.FirstOrDefault(p => p.Id == batch.PartId);
                    if (part == null) continue;

                    var key = new { PartId = part.Id, AlertType = "ExpiringSoon" };
                    var daysUntilExpiry = (batch.ExpiryDate!.Value - now).Days;
                    // ✅ FIX: Ensure daysUntilExpiry is not negative (shouldn't happen due to filter, but safety check)
                    if (daysUntilExpiry < 0) continue;
                    var severity = daysUntilExpiry <= 7 ? "Critical" : daysUntilExpiry <= 14 ? "High" : "Medium";

                    if (!existingAlertsDict.ContainsKey(key))
                    {
                        var alert = new InventoryAlert
                        {
                            PartId = part.Id,
                            AlertType = "ExpiringSoon",
                            Severity = severity,
                            Message = $"Phụ tùng {part.PartName ?? "N/A"} ({part.PartNumber ?? "N/A"}) - Lô {batch.BatchNumber ?? "N/A"} sắp hết hạn. Còn {daysUntilExpiry} ngày (Hết hạn: {batch.ExpiryDate.Value:dd/MM/yyyy})",
                            CurrentQuantity = batch.QuantityRemaining,
                            ExpiryDate = batch.ExpiryDate,
                            AlertDate = DateTime.UtcNow,
                            IsResolved = false
                        };
                        await unitOfWork.Repository<InventoryAlert>().AddAsync(alert);
                        newAlertsCount++;

                        await notificationService.NotifyInventoryAlertCreatedAsync(
                            "ExpiringSoon", part.Id, part.PartName ?? "", alert.Message);
                    }
                }

                // ✅ FIX: Resolve ExpiringSoon alerts for parts that no longer have expiring batches
                var partsWithExpiringBatches = expiringBatchesByPart.Select(b => b.PartId).ToHashSet();
                var expiringSoonAlerts = existingAlertsDict
                    .Where(kvp => kvp.Key.AlertType == "ExpiringSoon")
                    .ToList();
                
                foreach (var kvp in expiringSoonAlerts)
                {
                    if (!partsWithExpiringBatches.Contains(kvp.Key.PartId))
                    {
                        var alert = kvp.Value;
                        alert.IsResolved = true;
                        alert.ResolvedDate = DateTime.UtcNow;
                        await unitOfWork.Repository<InventoryAlert>().UpdateAsync(alert);
                        resolvedAlertsCount++;

                        var part = parts.FirstOrDefault(p => p.Id == kvp.Key.PartId);
                        if (part != null)
                        {
                            await notificationService.NotifyInventoryAlertResolvedAsync(part.Id, part.PartName ?? "");
                        }
                    }
                }

                await unitOfWork.SaveChangesAsync();

                // ✅ Performance: Use CountAsync instead of loading all alerts
                var totalUnresolvedCount = await unitOfWork.Repository<InventoryAlert>()
                    .CountAsync(a => !a.IsResolved);
                await notificationService.NotifyInventoryAlertUpdatedAsync(totalUnresolvedCount);

                _logger.LogInformation("Inventory alerts check completed. New: {NewCount}, Resolved: {ResolvedCount}, Total: {TotalCount}", 
                    newAlertsCount, resolvedAlertsCount, totalUnresolvedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in inventory alerts check job");
            }
            finally
            {
                // ✅ FIX: Always release lock
                _inventoryAlertsCheckLock.Release();
            }
        }

        public override void Dispose()
        {
            _maintenanceReminderTimer?.Dispose();
            _insuranceReminderTimer?.Dispose();
            _rateLimitCleanupTimer?.Dispose();
            _cacheCleanupTimer?.Dispose();
            _inventoryAlertsCheckTimer?.Dispose();
            _inventoryAlertsCheckLock?.Dispose(); // ✅ FIX: Dispose lock
            base.Dispose();
        }
    }
}

