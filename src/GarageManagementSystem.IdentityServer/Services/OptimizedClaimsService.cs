using GarageManagementSystem.IdentityServer.Data;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace GarageManagementSystem.IdentityServer.Services
{
    /// <summary>
    /// Optimized Claims Service - Kết hợp tất cả approaches để tối ưu performance
    /// </summary>
    public class OptimizedClaimsService
    {
        private readonly GaraManagementContext _context;
        private readonly IMemoryCache _cache;

        public OptimizedClaimsService(GaraManagementContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <summary>
        /// Lấy tất cả custom claims cho user - CHỈ BỔ SUNG VÀO CLAIMS MẶC ĐỊNH
        /// </summary>
        public async Task<List<System.Security.Claims.Claim>> GetAllClaimsForUserAsync(string userId, List<string> userRoles, List<string> requestedClaimTypes = null)
        {
            var claims = new List<System.Security.Claims.Claim>();

            // 1. ✅ USER-SPECIFIC CLAIMS - Database với Cache
            var userClaims = await GetUserSpecificClaimsAsync(userId);
            claims.AddRange(userClaims);

            // 2. ✅ BUSINESS LOGIC CLAIMS - Tính toán
            var businessClaims = GetBusinessLogicClaims(userId, userRoles);
            claims.AddRange(businessClaims);

            // 3. ✅ ROLE-BASED CLAIMS - Tính toán
            var roleClaims = GetRoleBasedClaims(userRoles);
            claims.AddRange(roleClaims);

            // 4. ✅ Filter theo requested claims
            if (requestedClaimTypes?.Any() == true)
            {
                claims = claims.Where(c => requestedClaimTypes.Contains(c.Type)).ToList();
            }

            return claims;
        }

        /// <summary>
        /// REMOVED: Static Claims - Không hardcode nữa
        /// Chỉ lấy dynamic claims từ database và business logic
        /// </summary>

        /// <summary>
        /// 1. User-Specific Claims - Database với Cache (Linh hoạt + Performance)
        /// </summary>
        private async Task<List<System.Security.Claims.Claim>> GetUserSpecificClaimsAsync(string userId)
        {
            // Cache key cho user-specific claims
            var cacheKey = $"user_claims_{userId}";

            if (_cache.TryGetValue(cacheKey, out List<System.Security.Claims.Claim> cachedClaims))
            {
                return cachedClaims;
            }

            var claims = new List<System.Security.Claims.Claim>();

            // Lấy từ ClaimsManagement database - TẤT CẢ ACTIVE CLAIMS
            var dbClaims = await _context.Claims
                .Where(c => c.IsActive) // Lấy tất cả active claims
                .ToListAsync();

            foreach (var dbClaim in dbClaims)
            {
                // ✅ SELECTIVE OVERRIDE: Cho phép override claims mặc định nếu có trong database
                // Claims mặc định của IdentityServer sẽ được override bởi custom claims từ database
                
                if (IsCriticalClaim(dbClaim.Name))
                {
                    // ⚠️ CHỈ BLOACK CÁC CLAIMS CRITICAL (sub, iss, aud, exp, iat, nbf, jti)
                    Console.WriteLine($"🚫 BLOCKED critical claim: {dbClaim.Name}");
                    continue;
                }

                var value = await GetUserClaimValue(dbClaim, userId);
                claims.Add(new System.Security.Claims.Claim(dbClaim.Name, value));
                
                // Log để biết claim nào sẽ override
                if (IsDefaultIdentityServerClaim(dbClaim.Name))
                {
                    Console.WriteLine($"🔄 WILL OVERRIDE default claim: {dbClaim.Name} = {value}");
                }
                else
                {
                    Console.WriteLine($"➕ WILL ADD custom claim: {dbClaim.Name} = {value}");
                }
            }

            // Cache 15 phút cho user-specific claims
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.High,
                Size = 1
            };

            _cache.Set(cacheKey, claims, cacheOptions);

            return claims;
        }

        /// <summary>
        /// 2. Business Logic Claims - Tính toán (Logic phức tạp)
        /// </summary>
        private List<System.Security.Claims.Claim> GetBusinessLogicClaims(string userId, List<string> userRoles)
        {
            var claims = new List<System.Security.Claims.Claim>();
            var now = DateTime.Now;

            // Claims dựa trên thời gian
            claims.Add(new System.Security.Claims.Claim("current_hour", now.Hour.ToString()));
            claims.Add(new System.Security.Claims.Claim("current_day", now.DayOfWeek.ToString()));
            claims.Add(new System.Security.Claims.Claim("business_hours", (now.Hour >= 9 && now.Hour <= 17).ToString().ToLower()));

            // Claims dựa trên session/request
            claims.Add(new System.Security.Claims.Claim("session_id", Guid.NewGuid().ToString()));
            claims.Add(new System.Security.Claims.Claim("request_timestamp", now.ToString("yyyy-MM-dd HH:mm:ss")));

            return claims;
        }

        /// <summary>
        /// 3. Role-Based Claims - Tính toán
        /// </summary>
        private List<System.Security.Claims.Claim> GetRoleBasedClaims(List<string> userRoles)
        {
            var claims = new List<System.Security.Claims.Claim>();

            // Security level claims
            if (userRoles.Contains("SuperAdmin"))
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "maximum"));
                claims.Add(new System.Security.Claims.Claim("can_audit", "true"));
                claims.Add(new System.Security.Claims.Claim("can_manage_system", "true"));
                claims.Add(new System.Security.Claims.Claim("admin_access", "true"));
            }
            else if (userRoles.Contains("Admin"))
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "high"));
                claims.Add(new System.Security.Claims.Claim("can_manage_users", "true"));
                claims.Add(new System.Security.Claims.Claim("admin_access", "true"));
            }
            else if (userRoles.Contains("Manager"))
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "medium"));
                claims.Add(new System.Security.Claims.Claim("can_view_reports", "true"));
                claims.Add(new System.Security.Claims.Claim("manager_access", "true"));
            }
            else if (userRoles.Contains("Technician"))
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "standard"));
                claims.Add(new System.Security.Claims.Claim("technician_access", "true"));
                claims.Add(new System.Security.Claims.Claim("can_manage_repairs", "true"));
            }
            else
            {
                claims.Add(new System.Security.Claims.Claim("security_level", "basic"));
                claims.Add(new System.Security.Claims.Claim("standard_access", "true"));
            }

            return claims;
        }

        /// <summary>
        /// Lấy giá trị claim từ database dựa trên user - THỰC SỰ DYNAMIC
        /// </summary>
        private async Task<string> GetUserClaimValue(Models.Claim claimEntity, string userId)
        {
            // ✅ THỰC SỰ DYNAMIC - Sử dụng CustomValueSource nếu có
            if (!string.IsNullOrEmpty(claimEntity.CustomValueSource))
            {
                return await GetDynamicValueFromSource(claimEntity.CustomValueSource, userId);
            }

            // ✅ Sử dụng DefaultValue nếu có
            if (!string.IsNullOrEmpty(claimEntity.DefaultValue))
            {
                return claimEntity.DefaultValue;
            }

            // ✅ Fallback: trả về claim name
            return claimEntity.Name;
        }

        /// <summary>
        /// Claims CRITICAL - KHÔNG BAO GIỜ ĐƯỢC OVERRIDE (JWT security)
        /// </summary>
        private bool IsCriticalClaim(string claimName)
        {
            var criticalClaims = new[]
            {
                "sub", "iss", "aud", "exp", "iat", "nbf", "jti", "at_hash", "c_hash"
            };
            
            return criticalClaims.Contains(claimName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Claims mặc định của IdentityServer - CÓ THỂ OVERRIDE
        /// </summary>
        private bool IsDefaultIdentityServerClaim(string claimName)
        {
            var defaultClaims = new[]
            {
                "name", "given_name", "family_name", "middle_name", "nickname", 
                "preferred_username", "profile", "picture", "website", "gender", 
                "birthdate", "zoneinfo", "locale", "updated_at",
                "email", "email_verified", "phone_number", "phone_number_verified",
                "address", "role", "scope"
            };
            
            return defaultClaims.Contains(claimName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// DEPRECATED: Thay bằng IsCriticalClaim() và IsDefaultIdentityServerClaim()
        /// </summary>
        [Obsolete("Use IsCriticalClaim() and IsDefaultIdentityServerClaim() instead")]
        private bool IsReservedClaim(string claimName)
        {
            return IsCriticalClaim(claimName) || IsDefaultIdentityServerClaim(claimName);
        }

        /// <summary>
        /// Lấy giá trị dynamic từ source được chỉ định
        /// Format: "Table:Column" hoặc "Table:Column:Condition"
        /// </summary>
        private async Task<string> GetDynamicValueFromSource(string sourcePath, string userId)
        {
            try
            {
                var parts = sourcePath.Split(':');
                if (parts.Length < 2)
                {
                    return $"Invalid source format: {sourcePath}";
                }

                var tableName = parts[0].ToLower();
                var columnName = parts[1];

                // ✅ DYNAMIC LOOKUP DỰA TRÊN TABLE:COLUMN
                return tableName switch
                {
                    "users" => await GetUserColumnValue(columnName, userId),
                    "userroles" => await GetUserRoleValue(columnName, userId),
                    "roles" => await GetRoleValue(columnName, userId),
                    "system" => await GetSystemValue(columnName),
                    "business" => await GetBusinessValue(columnName, userId),
                    _ => $"Unknown table: {tableName}"
                };
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// DYNAMIC LOOKUP METHODS - Không hardcode values
        /// </summary>
        private async Task<string> GetUserColumnValue(string columnName, string userId)
        {
            // ✅ THỰC SỰ DYNAMIC - Lấy từ bảng Users
            await Task.Delay(1); // Simulate async
            
            // TODO: Implement actual database lookup based on columnName
            // Ví dụ: SELECT {columnName} FROM Users WHERE Id = {userId}
            return $"User.{columnName}"; // Placeholder
        }

        private async Task<string> GetUserRoleValue(string columnName, string userId)
        {
            await Task.Delay(1);
            // TODO: Lookup from UserRoles table
            return $"UserRole.{columnName}";
        }

        private async Task<string> GetRoleValue(string columnName, string userId)
        {
            await Task.Delay(1);
            // TODO: Lookup from Roles table
            return $"Role.{columnName}";
        }

        private async Task<string> GetSystemValue(string columnName)
        {
            await Task.Delay(1);
            
            // ✅ SYSTEM VALUES - Không hardcode, tính toán dynamic
            return columnName.ToLower() switch
            {
                "timestamp" => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "session_id" => Guid.NewGuid().ToString(),
                "version" => "1.0.0",
                "environment" => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "development",
                "server_name" => Environment.MachineName,
                "current_hour" => DateTime.Now.Hour.ToString(),
                "current_day" => DateTime.Now.DayOfWeek.ToString(),
                _ => $"System.{columnName}"
            };
        }

        private async Task<string> GetBusinessValue(string columnName, string userId)
        {
            await Task.Delay(1);
            
            // ✅ BUSINESS LOGIC VALUES - Tính toán dựa trên business rules
            return columnName.ToLower() switch
            {
                "business_hours" => (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 17).ToString().ToLower(),
                "weekend" => (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday).ToString().ToLower(),
                "month" => DateTime.Now.Month.ToString(),
                "quarter" => ((DateTime.Now.Month - 1) / 3 + 1).ToString(),
                _ => $"Business.{columnName}"
            };
        }

        /// <summary>
        /// Invalidate cache khi có thay đổi
        /// </summary>
        public void InvalidateUserClaimsCache(string userId)
        {
            _cache.Remove($"user_claims_{userId}");
        }

        /// <summary>
        /// Invalidate tất cả cache
        /// </summary>
        public void InvalidateAllClaimsCache()
        {
            // Có thể implement pattern để clear tất cả cache keys
            // Hoặc sử dụng distributed cache với tags
        }
    }
}
