using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Concurrent;

namespace GarageManagementSystem.API.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task RemoveByPrefixAsync(string prefix);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null);
        void Remove(string key);
        void RemoveByPrefix(string pattern);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            await Task.CompletedTask;
            return _memoryCache.TryGetValue(key, out T? value) ? value : default(T);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            await Task.CompletedTask;
            _memoryCache.Set(key, value, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            await Task.CompletedTask;
            _memoryCache.Remove(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            await Task.CompletedTask;
            // For simplicity, we'll implement a basic pattern matching
            // In production, consider using Redis or a more sophisticated cache
            var cacheEntries = _memoryCache as MemoryCache;
            if (cacheEntries != null)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var coherentState = field?.GetValue(cacheEntries);
                var entriesCollection = coherentState?.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = entriesCollection?.GetValue(coherentState) as IDictionary;

                if (entries != null)
                {
                    var keysToRemove = new List<object>();
                    foreach (DictionaryEntry entry in entries)
                    {
                        if (entry.Key.ToString()?.Contains(pattern.Replace("*", "")) == true)
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }

                    foreach (var key in keysToRemove)
                    {
                        _memoryCache.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// ✅ HP2: Remove all cache entries with the given prefix
        /// </summary>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            await Task.CompletedTask;
            var cacheEntries = _memoryCache as MemoryCache;
            if (cacheEntries != null)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var coherentState = field?.GetValue(cacheEntries);
                var entriesCollection = coherentState?.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = entriesCollection?.GetValue(coherentState) as IDictionary;

                if (entries != null)
                {
                    var keysToRemove = new List<object>();
                    foreach (DictionaryEntry entry in entries)
                    {
                        if (entry.Key.ToString()?.StartsWith(prefix) == true)
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }

                    foreach (var key in keysToRemove)
                    {
                        _memoryCache.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Get or set with thread-safe locking
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            
            await semaphore.WaitAsync();
            try
            {
                var cached = await GetAsync<T>(key);
                if (cached != null)
                {
                    return cached;
                }

                var value = await factory();
                await SetAsync(key, value, absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5));
                return value;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public void RemoveByPrefix(string pattern)
        {
            var cacheEntries = _memoryCache as MemoryCache;
            if (cacheEntries != null)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var coherentState = field?.GetValue(cacheEntries);
                var entriesCollection = coherentState?.GetType().GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = entriesCollection?.GetValue(coherentState) as IDictionary;

                if (entries != null)
                {
                    var keysToRemove = new List<object>();
                    foreach (DictionaryEntry entry in entries)
                    {
                        if (entry.Key.ToString()?.StartsWith(pattern) == true)
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }

                    foreach (var key in keysToRemove)
                    {
                        _memoryCache.Remove(key);
                    }
                }
            }
        }
    }
}
