using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GarageManagementSystem.Core.Services
{
    /// <summary>
    /// Caching service for improving performance
    /// </summary>
    public interface ICacheService
    {
        T? Get<T>(string key);
        Task<T?> GetAsync<T>(string key);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        void Remove(string key);
        Task RemoveAsync(string key);
        void RemoveByPrefix(string prefix);
        Task RemoveByPrefixAsync(string prefix);
        bool Exists(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly HashSet<string> _cacheKeys = new();
        private readonly object _lock = new();

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T? Get<T>(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out T? value))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return value;
                }
                
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for key: {Key}", key);
                return default;
            }
        }

        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(Get<T>(key));
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }
                else
                {
                    // Default 30 minutes
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                // Add callback to remove from tracking when evicted
                options.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    lock (_lock)
                    {
                        _cacheKeys.Remove(key.ToString()!);
                    }
                });

                _cache.Set(key, value, options);
                
                lock (_lock)
                {
                    _cacheKeys.Add(key);
                }

                _logger.LogDebug("Cache set for key: {Key}, expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            Set(key, value, expiration);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            try
            {
                _cache.Remove(key);
                
                lock (_lock)
                {
                    _cacheKeys.Remove(key);
                }

                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void RemoveByPrefix(string prefix)
        {
            try
            {
                List<string> keysToRemove;
                
                lock (_lock)
                {
                    keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
                }

                foreach (var key in keysToRemove)
                {
                    Remove(key);
                }

                _logger.LogInformation("Removed {Count} cache entries with prefix: {Prefix}", keysToRemove.Count, prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
            }
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            RemoveByPrefix(prefix);
            return Task.CompletedTask;
        }

        public bool Exists(string key)
        {
            return _cache.TryGetValue(key, out _);
        }
    }

    /// <summary>
    /// Cache key constants
    /// </summary>
    public static class CacheKeys
    {
        // Customers
        public const string AllCustomers = "customers:all";
        public const string Customer = "customer:{0}";
        
        // Vehicles
        public const string AllVehicles = "vehicles:all";
        public const string Vehicle = "vehicle:{0}";
        public const string CustomerVehicles = "vehicles:customer:{0}";
        
        // Parts
        public const string AllParts = "parts:all";
        public const string Part = "part:{0}";
        public const string LowStockParts = "parts:lowstock";
        public const string PartsByCategory = "parts:category:{0}";
        
        // Services
        public const string AllServices = "services:all";
        public const string Service = "service:{0}";
        
        // Employees
        public const string AllEmployees = "employees:all";
        public const string Employee = "employee:{0}";
        
        // Reports
        public const string RevenueReport = "reports:revenue:{0}:{1}"; // from:to
        public const string DashboardStats = "reports:dashboard";
        public const string TopCustomers = "reports:topcustomers:{0}"; // days
        
        // Configuration
        public const string SystemConfig = "config:{0}";
        public const string AllConfigs = "config:all";
        
        // Inventory
        public const string InventoryStatus = "inventory:status";
        public const string StockAlerts = "inventory:alerts";

        // Cache expiration times
        public static readonly TimeSpan ShortCache = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MediumCache = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan LongCache = TimeSpan.FromHours(1);
        public static readonly TimeSpan VeryLongCache = TimeSpan.FromHours(24);
    }
}

