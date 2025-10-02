using GarageManagementSystem.IdentityServer.Data;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace GarageManagementSystem.IdentityServer.Services
{
    /// <summary>
    /// Service để quản lý custom claims từ database
    /// </summary>
    public class CustomClaimsService
    {
        private readonly GaraManagementContext _context;
        private readonly IMemoryCache _cache;

        public CustomClaimsService(GaraManagementContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <summary>
        /// Lấy custom claims từ ClaimsManagement database
        /// </summary>
        public async Task<List<System.Security.Claims.Claim>> GetCustomClaimsForUserAsync(string userId, List<string> requestedClaimTypes = null)
        {
            const string cacheKey = "custom_claims_all";

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedClaims))
            {
                return FilterClaims(cachedClaims as List<System.Security.Claims.Claim>, requestedClaimTypes);
            }

            // Get active claims from database - ĐÂY LÀ PHẦN QUAN TRỌNG!
            var claimEntities = await _context.Claims
                .Where(c => c.IsActive)
                .ToListAsync();

            // Convert database claims to Claim objects
            var claims = new List<System.Security.Claims.Claim>();
            foreach (var claimEntity in claimEntities)
            {
                // Lấy giá trị động dựa trên claim type và user
                var claimValue = GetClaimValue(claimEntity, userId);
                claims.Add(new System.Security.Claims.Claim(claimEntity.Name, claimValue));
            }

            // Cache for 30 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1
            };

            _cache.Set(cacheKey, claims, cacheOptions);

            return FilterClaims(claims, requestedClaimTypes);
        }

        /// <summary>
        /// Lấy custom claims cho client cụ thể
        /// </summary>
        public async Task<List<System.Security.Claims.Claim>> GetCustomClaimsForClientAsync(string clientId)
        {
            // Ví dụ: Custom claims dựa trên client
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("clientname", clientId == "garage.web" ? "abc" : "default"),
                new System.Security.Claims.Claim("client_id", clientId),
                new System.Security.Claims.Claim("client_type", clientId.Contains("web") ? "web_application" : "api_client"),
                new System.Security.Claims.Claim("deployment_environment", "development"),
                new System.Security.Claims.Claim("api_version", "v1"),
                new System.Security.Claims.Claim("supported_features", "full_access")
            };

            return claims;
        }

        /// <summary>
        /// Lấy custom claims dựa trên business logic
        /// </summary>
        public List<System.Security.Claims.Claim> GetBusinessLogicClaims(ApplicationUser user, List<string> userRoles)
        {
            var claims = new List<System.Security.Claims.Claim>();

            // Claims dựa trên user properties
            if (!string.IsNullOrEmpty(user.Email))
            {
                var domain = user.Email.Split('@').LastOrDefault();
                claims.Add(new System.Security.Claims.Claim("email_domain", domain ?? ""));
                
                if (domain == "company.com")
                {
                    claims.Add(new System.Security.Claims.Claim("is_employee", "true"));
                    claims.Add(new System.Security.Claims.Claim("company_access", "full"));
                }
            }

            // Claims dựa trên roles
            if (userRoles.Contains("SuperAdmin"))
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "maximum"));
                claims.Add(new System.Security.Claims.Claim("can_audit", "true"));
                claims.Add(new System.Security.Claims.Claim("can_manage_system", "true"));
            }
            else if (userRoles.Contains("Admin"))
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "high"));
                claims.Add(new System.Security.Claims.Claim("can_manage_users", "true"));
            }
            else if (userRoles.Contains("Manager"))
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "medium"));
                claims.Add(new System.Security.Claims.Claim("can_view_reports", "true"));
            }
            else
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "standard"));
            }

            // Claims dựa trên user status
            claims.Add(new System.Security.Claims.Claim("account_age_days", DateTime.Now.Subtract(user.CreatedAt).Days.ToString()));
            
            if (user.IsActive)
            {
                claims.Add(new System.Security.Claims.Claim("account_status", "active"));
            }
            else
            {
                claims.Add(new System.Security.Claims.Claim("account_status", "suspended"));
            }

            // Claims dựa trên thời gian
            var currentHour = DateTime.Now.Hour;
            if (currentHour >= 9 && currentHour <= 17)
            {
                claims.Add(new System.Security.Claims.Claim("business_hours", "true"));
            }
            else
            {
                claims.Add(new System.Security.Claims.Claim("business_hours", "false"));
            }

            return claims;
        }

        /// <summary>
        /// Lấy giá trị claim dựa trên loại claim từ ClaimsManagement
        /// </summary>
        private string GetClaimValue(GarageManagementSystem.IdentityServer.Models.Claim claimEntity, string userId)
        {
            // Logic để generate claim value dựa trên claim type và user
            switch (claimEntity.Name.ToLower())
            {
                case "clientname":
                    return "abc"; // Giá trị bạn muốn
                
                case "user_id":
                    return userId;
                
                case "timestamp":
                    return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                case "session_id":
                    return Guid.NewGuid().ToString();
                
                case "request_id":
                    return Guid.NewGuid().ToString();
                
                case "organization":
                    return "Garage Management System";
                
                case "department":
                    return "IT Department";
                
                case "environment":
                    return "development";
                
                case "version":
                    return "1.0.0";
                
                case "timezone":
                    return "UTC+7";
                
                case "language":
                    return "vi-VN";
                
                default:
                    // Cho các claims khác, có thể:
                    // 1. Return static value từ database nếu có
                    // 2. Generate dynamic value
                    // 3. Return claim name as fallback
                    return claimEntity.Name; // Fallback
            }
        }

        /// <summary>
        /// Filter claims dựa trên requested claim types
        /// </summary>
        private List<System.Security.Claims.Claim> FilterClaims(List<System.Security.Claims.Claim> claims, List<string> requestedClaimTypes)
        {
            if (requestedClaimTypes == null || !requestedClaimTypes.Any())
                return claims;

            return claims.Where(c => requestedClaimTypes.Contains(c.Type)).ToList();
        }

        /// <summary>
        /// Invalidate cache khi có thay đổi claims
        /// </summary>
        public void InvalidateClaimsCache()
        {
            _cache.Remove("custom_claims_all");
        }
    }
}
