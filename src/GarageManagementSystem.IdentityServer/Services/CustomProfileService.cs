using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using GarageManagementSystem.IdentityServer.Models;
using System.Security.Claims;

namespace GarageManagementSystem.IdentityServer.Services
{
    /// <summary>
    /// Custom Profile Service để thêm custom claims tự động
    /// </summary>
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly OptimizedClaimsService _optimizedClaimsService;

        public CustomProfileService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            OptimizedClaimsService optimizedClaimsService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _optimizedClaimsService = optimizedClaimsService;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            Console.WriteLine("🔍 CustomProfileService.GetProfileDataAsync called!");
            
            var user = await _userManager.GetUserAsync(context.Subject);
            if (user == null) 
            {
                Console.WriteLine("❌ User is null!");
                return;
            }
            
            Console.WriteLine($"👤 User found: {user.Email}");

            // ✅ GIỮ NGUYÊN CLAIMS MẶC ĐỊNH CỦA IDENTITYSERVER
            // IdentityServer4 sẽ tự động thêm basic claims (sub, name, email, etc.)
            // và role claims từ AddAspNetIdentity

            // ✅ CHỈ BỔ SUNG CUSTOM CLAIMS
            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Lấy custom claims từ OptimizedClaimsService
            var customClaims = await _optimizedClaimsService.GetAllClaimsForUserAsync(
                user.Id, 
                userRoles.ToList(), 
                context.RequestedClaimTypes?.ToList());

            // ✅ CHỈ THÊM CUSTOM CLAIMS VÀO CONTEXT
            // Không override claims mặc định
            context.IssuedClaims.AddRange(customClaims);

            // ✅ Log custom claims for debugging
            Console.WriteLine($"Added {customClaims.Count} custom claims for user {user.Email}:");
            foreach (var claim in customClaims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject);
            
            // User is active if:
            // 1. User exists
            // 2. User is not locked out
            // 3. User.IsActive is true
            context.IsActive = user != null && 
                              !await _userManager.IsLockedOutAsync(user) && 
                              user.IsActive;
        }
    }
}
