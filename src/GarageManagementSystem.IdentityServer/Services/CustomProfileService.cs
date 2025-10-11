using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using GarageManagementSystem.IdentityServer.Models;
using System.Security.Claims;

namespace GarageManagementSystem.IdentityServer.Services
{
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly OptimizedClaimsService _claimsService;

        public CustomProfileService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            OptimizedClaimsService claimsService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _claimsService = claimsService;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            if (user == null)
            {
                return;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            // Get all claims for the user using the optimized service
            var allClaims = await _claimsService.GetAllClaimsForUserAsync(user.Id, userRoles.ToList(), context.RequestedClaimTypes?.ToList());

            // Use all claims directly since they're already filtered by the service
            var requestedClaims = allClaims;

            // Add the filtered claims to the context
            context.IssuedClaims.AddRange(requestedClaims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            context.IsActive = user != null && user.IsActive;
        }
    }
}
