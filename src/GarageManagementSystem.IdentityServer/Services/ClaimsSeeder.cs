using GarageManagementSystem.IdentityServer.Data;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.IdentityServer.Services
{
    /// <summary>
    /// Service để seed claims vào ClaimsManagement database
    /// </summary>
    public class ClaimsSeeder
    {
        private readonly GaraManagementContext _context;

        public ClaimsSeeder(GaraManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Seed các custom claims vào database
        /// </summary>
        public async Task SeedCustomClaimsAsync()
        {
            var existingClaims = await _context.Claims.Select(c => c.Name).ToListAsync();

            var customClaims = new List<Models.Claim>
            {
                new Models.Claim
                {
                    Name = "clientname",
                    DisplayName = "Client Name",
                    Description = "Name of the client application",
                    Category = "Client",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "organization",
                    DisplayName = "Organization",
                    Description = "Organization name",
                    Category = "Organization",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "department",
                    DisplayName = "Department",
                    Description = "User department",
                    Category = "Organization",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "environment",
                    DisplayName = "Environment",
                    Description = "Application environment",
                    Category = "System",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "version",
                    DisplayName = "Version",
                    Description = "Application version",
                    Category = "System",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "timezone",
                    DisplayName = "Timezone",
                    Description = "User timezone",
                    Category = "User",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "language",
                    DisplayName = "Language",
                    Description = "User preferred language",
                    Category = "User",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "session_id",
                    DisplayName = "Session ID",
                    Description = "Current session identifier",
                    Category = "Session",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "request_id",
                    DisplayName = "Request ID",
                    Description = "Current request identifier",
                    Category = "Request",
                    IsStandard = false,
                    IsActive = true
                },
                new Models.Claim
                {
                    Name = "timestamp",
                    DisplayName = "Timestamp",
                    Description = "Current timestamp",
                    Category = "System",
                    IsStandard = false,
                    IsActive = true
                }
            };

            foreach (var claim in customClaims)
            {
                if (!existingClaims.Contains(claim.Name))
                {
                    _context.Claims.Add(claim);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
