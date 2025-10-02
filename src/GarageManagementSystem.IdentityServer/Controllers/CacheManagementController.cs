using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class CacheManagementController : Controller
    {
        private readonly IMemoryCache _cache;

        public CacheManagementController(IMemoryCache cache)
        {
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        // POST: CacheManagement/TriggerAllCaches - Trigger tất cả cache endpoints để tạo cache
        [HttpPost]
        public async Task<IActionResult> TriggerAllCaches()
        {
            try
            {
                var triggeredCaches = new List<string>();

                // Trigger Claims Management cache
                var claimsResponse = await TriggerCacheEndpoint("/ClaimsManagement/GetAvailableClaims");
                if (claimsResponse) triggeredCaches.Add("Claims Management");

                // Trigger API Scope Management cache
                var apiScopeResponse = await TriggerCacheEndpoint("/ApiScopeManagement/GetAvailableClaims");
                if (apiScopeResponse) triggeredCaches.Add("API Scope Management");

                // Trigger Identity Resource Management cache
                var identityResourceResponse = await TriggerCacheEndpoint("/IdentityResourceManagement/GetAvailableClaims");
                if (identityResourceResponse) triggeredCaches.Add("Identity Resource Management");

                        // Trigger API Resource Management cache (multiple endpoints)
                        var apiResourceClaimsResponse = await TriggerCacheEndpoint("/ApiResourceManagement/GetAvailableClaims");
                        var apiResourceScopesResponse = await TriggerCacheEndpoint("/ApiResourceManagement/GetAvailableScopes");
                        var apiResourceApiScopesResponse = await TriggerCacheEndpoint("/ApiResourceManagement/GetApiScopes");
                        var apiResourceListResponse = await TriggerCacheEndpoint("/ApiResourceManagement/GetApiResources");
                        if (apiResourceClaimsResponse || apiResourceScopesResponse || apiResourceApiScopesResponse || apiResourceListResponse) 
                            triggeredCaches.Add("API Resource Management");

                        // Trigger Client Management cache (multiple endpoints)
                        var clientClaimsResponse = await TriggerCacheEndpoint("/ClientManagement/GetAvailableClaims");
                        var clientScopesResponse = await TriggerCacheEndpoint("/ClientManagement/GetAvailableScopes");
                        var clientListResponse = await TriggerCacheEndpoint("/ClientManagement/GetClients");
                        if (clientClaimsResponse || clientScopesResponse || clientListResponse) 
                            triggeredCaches.Add("Client Management");

                // Trigger User Management cache
                var userListResponse = await TriggerCacheEndpoint("/UserManagement/GetUsers");
                if (userListResponse) triggeredCaches.Add("User Management");

                // Trigger Role Management cache
                var roleListResponse = await TriggerCacheEndpoint("/RoleManagement/GetRoles");
                var availableRolesResponse = await TriggerCacheEndpoint("/RoleManagement/GetAvailableRoles");
                if (roleListResponse || availableRolesResponse) triggeredCaches.Add("Role Management");

                return Json(new { 
                    success = true, 
                    message = "All cache endpoints triggered successfully!", 
                    triggeredCaches = triggeredCaches 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error triggering caches: {ex.Message}" });
            }
        }

        // Helper method to trigger cache endpoint
        private async Task<bool> TriggerCacheEndpoint(string endpoint)
        {
            try
            {
                using var httpClient = new HttpClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var response = await httpClient.GetAsync($"{baseUrl}{endpoint}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // POST: CacheManagement/PreloadCache - Tạo cache mẫu để test
        [HttpPost]
        public IActionResult PreloadCache()
        {
            try
            {
                var preloadedItems = new List<string>();

                // Preload sample cache entries with proper Size configuration
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                    Priority = CacheItemPriority.High,
                    Size = 1 // Each cache entry counts as 1 unit toward the size limit
                };

                var sampleClaims = new[] { new { value = "sample_claim", text = "Sample Claim" } };
                _cache.Set("available_claims", sampleClaims, cacheOptions);
                preloadedItems.Add("available_claims");

                var sampleApiScopes = new[] { new { value = "sample_scope", text = "Sample API Scope" } };
                _cache.Set("available_claims_for_api_scope", sampleApiScopes, cacheOptions);
                preloadedItems.Add("available_claims_for_api_scope");

                var sampleIdentityResources = new[] { new { value = "sample_identity", text = "Sample Identity Resource" } };
                _cache.Set("available_claims_for_identity_resource", sampleIdentityResources, cacheOptions);
                preloadedItems.Add("available_claims_for_identity_resource");

                var sampleApiResources = new[] { new { value = "sample_api", text = "Sample API Resource" } };
                _cache.Set("available_claims_for_api_resource", sampleApiResources, cacheOptions);
                preloadedItems.Add("available_claims_for_api_resource");

                var sampleApiScopesList = new[] { new { value = "scope1", text = "API Scope 1" }, new { value = "scope2", text = "API Scope 2" } };
                _cache.Set("available_api_scopes", sampleApiScopesList, cacheOptions);
                preloadedItems.Add("available_api_scopes");

                var sampleIdentityResourcesList = new[] { new { value = "identity1", text = "Identity Resource 1" } };
                _cache.Set("all_identity_resources", sampleIdentityResourcesList, cacheOptions);
                preloadedItems.Add("all_identity_resources");

                var sampleApiResourcesList = new[] { new { value = "api1", text = "API Resource 1" } };
                _cache.Set("all_api_resources", sampleApiResourcesList, cacheOptions);
                preloadedItems.Add("all_api_resources");

                        var sampleClients = new[] { new { value = "client1", text = "Sample Client" } };
                        _cache.Set("available_clients", sampleClients, cacheOptions);
                        preloadedItems.Add("available_clients");

                        var sampleUsers = new[] { new { value = "user1", text = "Sample User" } };
                        _cache.Set("all_users", sampleUsers, cacheOptions);
                        preloadedItems.Add("all_users");

                        var sampleRoles = new[] { new { value = "role1", text = "Sample Role" } };
                        _cache.Set("all_roles", sampleRoles, cacheOptions);
                        preloadedItems.Add("all_roles");

                        var sampleAvailableRoles = new[] { new { value = "Admin", text = "Admin - Administrator Role" } };
                        _cache.Set("available_roles", sampleAvailableRoles, cacheOptions);
                        preloadedItems.Add("available_roles");

                return Json(new { 
                    success = true, 
                    message = "Cache preloaded successfully!", 
                    preloadedItems = preloadedItems 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error preloading cache: {ex.Message}" });
            }
        }

        // GET: CacheManagement/GetCacheInfo
        [HttpGet]
        public IActionResult GetCacheInfo()
        {
            try
            {
                var cacheInfo = new
                {
                    // Claims Management Cache
                    claimsCache = GetCacheStatus("available_claims"),
                    claimsApiScopeCache = GetCacheStatus("available_claims_for_api_scope"),
                    claimsIdentityResourceCache = GetCacheStatus("available_claims_for_identity_resource"),
                    claimsApiResourceCache = GetCacheStatus("available_claims_for_api_resource"),
                    
                    // API Scopes Cache
                    apiScopesCache = GetCacheStatus("available_scopes_for_api_resource"),
                    allApiScopesCache = GetCacheStatus("available_api_scopes"),
                    
                    // Identity Resources Cache
                    identityResourcesCache = GetCacheStatus("available_claims_for_identity_resource"),
                    allIdentityResourcesCache = GetCacheStatus("all_identity_resources"),
                    
                    // API Resources Cache
                    apiResourcesCache = GetCacheStatus("available_claims_for_api_resource"),
                    allApiResourcesCache = GetCacheStatus("available_api_resources"),
                    
                    // Clients Cache
                    clientsCache = GetCacheStatus("available_clients"),
                    allClientsCache = GetCacheStatus("all_clients"),
                    
                    // Users Cache
                    usersCache = GetCacheStatus("all_users"),
                    
                    // Roles Cache
                    rolesCache = GetCacheStatus("all_roles"),
                    availableRolesCache = GetCacheStatus("available_roles"),
                    
                    // Cache Statistics
                    totalCachedItems = GetTotalCachedItems(),
                    memoryUsage = GetMemoryUsageInfo()
                };

                return Json(new { success = true, data = cacheInfo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: CacheManagement/ClearAllCache
        [HttpPost]
        public IActionResult ClearAllCache()
        {
            try
            {
                // Clear all cache entries
                if (_cache is MemoryCache memoryCache)
                {
                    var field = typeof(MemoryCache).GetField("_coherentState", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var coherentState = field?.GetValue(memoryCache);
                    var entriesCollection = coherentState?.GetType()
                        .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var entries = entriesCollection?.GetValue(coherentState) as System.Collections.IDictionary;
                    
                    if (entries != null)
                    {
                        var keys = new List<object>();
                        foreach (System.Collections.DictionaryEntry entry in entries)
                        {
                            keys.Add(entry.Key);
                        }
                        
                        foreach (var key in keys)
                        {
                            memoryCache.Remove(key);
                        }
                    }
                }

                return Json(new { success = true, message = "All cache cleared successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error clearing cache: {ex.Message}" });
            }
        }

        // POST: CacheManagement/ClearSpecificCache
        [HttpPost]
        public IActionResult ClearSpecificCache([FromBody] ClearCacheRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CacheKey))
                {
                    return Json(new { success = false, message = "Cache key is required" });
                }

                _cache.Remove(request.CacheKey);

                return Json(new { success = true, message = $"Cache '{request.CacheKey}' cleared successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error clearing cache: {ex.Message}" });
            }
        }

        // POST: CacheManagement/ClearCacheByType
        [HttpPost]
        public IActionResult ClearCacheByType([FromBody] ClearCacheByTypeRequest request)
        {
            try
            {
                var clearedItems = new List<string>();

                switch (request.CacheType.ToLower())
                {
                    case "claims":
                        var claimsKeys = new[] { "available_claims", "available_claims_for_api_scope", "available_claims_for_identity_resource", "available_claims_for_api_resource" };
                        foreach (var key in claimsKeys)
                        {
                            _cache.Remove(key);
                            clearedItems.Add(key);
                        }
                        break;

                    case "api_scopes":
                        var apiScopeKeys = new[] { "available_scopes_for_api_resource", "available_api_scopes" };
                        foreach (var key in apiScopeKeys)
                        {
                            _cache.Remove(key);
                            clearedItems.Add(key);
                        }
                        break;

                    case "identity_resources":
                        var identityResourceKeys = new[] { "available_claims_for_identity_resource", "all_identity_resources" };
                        foreach (var key in identityResourceKeys)
                        {
                            _cache.Remove(key);
                            clearedItems.Add(key);
                        }
                        break;

                    case "api_resources":
                        var apiResourceKeys = new[] { "available_claims_for_api_resource", "available_scopes_for_api_resource", "available_api_resources" };
                        foreach (var key in apiResourceKeys)
                        {
                            _cache.Remove(key);
                            clearedItems.Add(key);
                        }
                        break;

                    case "clients":
                        var clientKeys = new[] { "available_clients", "all_clients", "available_scopes_for_client" };
                        foreach (var key in clientKeys)
                        {
                            _cache.Remove(key);
                            clearedItems.Add(key);
                        }
                        break;

                    case "users":
                        var userKeys = new[] { "all_users" };
                        foreach (var key in userKeys)
                        {
                            _cache.Remove(key);
                            clearedItems.Add(key);
                        }
                        break;

                    case "roles":
                        var roleKeys = new[] { "all_roles", "available_roles" };
                        foreach (var key in roleKeys)
                        {
                            _cache.Remove(key);
                            clearedItems.Add(key);
                        }
                        break;

                    default:
                        return Json(new { success = false, message = "Invalid cache type" });
                }

                return Json(new { 
                    success = true, 
                    message = $"Cache type '{request.CacheType}' cleared successfully!", 
                    clearedItems = clearedItems 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error clearing cache: {ex.Message}" });
            }
        }

        // Helper methods
        private string GetCacheStatus(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out var value))
            {
                return "Active";
            }
            return "Not Cached";
        }

        private int GetTotalCachedItems()
        {
            try
            {
                if (_cache is MemoryCache memoryCache)
                {
                    var field = typeof(MemoryCache).GetField("_coherentState", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var coherentState = field?.GetValue(memoryCache);
                    var entriesCollection = coherentState?.GetType()
                        .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var entries = entriesCollection?.GetValue(coherentState) as System.Collections.IDictionary;
                    
                    return entries?.Count ?? 0;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private object GetMemoryUsageInfo()
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                return new
                {
                    workingSet = process.WorkingSet64,
                    privateMemory = process.PrivateMemorySize64,
                    virtualMemory = process.VirtualMemorySize64
                };
            }
            catch
            {
                return new { workingSet = 0, privateMemory = 0, virtualMemory = 0 };
            }
        }
    }

    public class ClearCacheRequest
    {
        public string CacheKey { get; set; } = string.Empty;
    }

    public class ClearCacheByTypeRequest
    {
        public string CacheType { get; set; } = string.Empty; // claims, api_scopes, identity_resources, api_resources, clients, users, roles
    }
}
