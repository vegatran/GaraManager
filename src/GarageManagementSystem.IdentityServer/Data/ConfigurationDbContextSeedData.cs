using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GarageManagementSystem.IdentityServer.Data
{
    public static class ConfigurationDbContextSeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed API Scopes
            await SeedApiScopesAsync(context);

            // Seed Identity Resources
            await SeedIdentityResourcesAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedApiScopesAsync(ConfigurationDbContext context)
        {
            var existingScopes = await context.ApiScopes.Select(s => s.Name).ToListAsync();

            var apiScopes = new[]
            {
                new ApiScope
                {
                    Name = "garage.api",
                    DisplayName = "Garage API",
                    Description = "Full access to Garage Management System API",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Name = "garage.api.read",
                    DisplayName = "Garage API - Read Only",
                    Description = "Read-only access to Garage Management System API",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Name = "garage.api.write",
                    DisplayName = "Garage API - Write",
                    Description = "Write access to Garage Management System API",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Name = "garage.api.admin",
                    DisplayName = "Garage API - Admin",
                    Description = "Administrative access to Garage Management System API",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Name = "admin.api",
                    DisplayName = "Admin API",
                    Description = "Administrative API access",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Name = "user.api",
                    DisplayName = "User API",
                    Description = "User management API access",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Name = "notification.api",
                    DisplayName = "Notification API",
                    Description = "Notification service API access",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                },
                new ApiScope
                {
                    Name = "report.api",
                    DisplayName = "Report API",
                    Description = "Reporting service API access",
                    Enabled = true,
                    ShowInDiscoveryDocument = true
                }
            };

            foreach (var scope in apiScopes)
            {
                if (!existingScopes.Contains(scope.Name))
                {
                    context.ApiScopes.Add(scope);
                }
            }
        }

        private static async Task SeedIdentityResourcesAsync(ConfigurationDbContext context)
        {
            var existingResources = await context.IdentityResources.Select(r => r.Name).ToListAsync();

            var identityResources = new[]
            {
                new IdentityResource
                {
                    Name = "openid",
                    DisplayName = "OpenID",
                    Description = "OpenID Connect identity",
                    Required = true,
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityResourceClaim> { new IdentityResourceClaim { Type = "sub" } }
                },
                new IdentityResource
                {
                    Name = "profile",
                    DisplayName = "Profile",
                    Description = "User profile information",
                    Required = false,
                    ShowInDiscoveryDocument = true
                },
                new IdentityResource
                {
                    Name = "email",
                    DisplayName = "Email",
                    Description = "User email address",
                    Required = false,
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityResourceClaim> 
                    { 
                        new IdentityResourceClaim { Type = "email" }, 
                        new IdentityResourceClaim { Type = "email_verified" } 
                    }
                },
                new IdentityResource
                {
                    Name = "address",
                    DisplayName = "Address",
                    Description = "User address information",
                    Required = false,
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityResourceClaim> 
                    { 
                        new IdentityResourceClaim { Type = "address" } 
                    }
                },
                new IdentityResource
                {
                    Name = "phone",
                    DisplayName = "Phone",
                    Description = "User phone number",
                    Required = false,
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityResourceClaim> 
                    { 
                        new IdentityResourceClaim { Type = "phone_number" }, 
                        new IdentityResourceClaim { Type = "phone_number_verified" } 
                    }
                },
                new IdentityResource
                {
                    Name = "roles",
                    DisplayName = "Roles",
                    Description = "User roles",
                    Required = false,
                    ShowInDiscoveryDocument = true,
                    UserClaims = new List<IdentityResourceClaim> 
                    { 
                        new IdentityResourceClaim { Type = "role" } 
                    }
                }
            };

            foreach (var resource in identityResources)
            {
                if (!existingResources.Contains(resource.Name))
                {
                    context.IdentityResources.Add(resource);
                }
            }
        }
    }
}
