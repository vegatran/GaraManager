using GarageManagementSystem.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Duende.IdentityServer.EntityFramework.DbContexts;
using GarageManagementSystem.IdentityServer.Data;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Extensions;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ConfigurationDbContext _configContext;
        private readonly PersistedGrantDbContext _persistedGrantContext;

        public SetupController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ConfigurationDbContext configContext,
            PersistedGrantDbContext persistedGrantContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configContext = configContext;
            _persistedGrantContext = persistedGrantContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InitializeSystem()
        {
            try
            {
                // 1. Clear existing demo data first
                await ClearIdentityServerData();

                // 2. Seed Identity Resources
                await SeedIdentityResources();

                // 3. Seed API Scopes
                await SeedApiScopes();

                // 4. Seed API Resources
                await SeedApiResources();

                // 5. Seed Clients
                await SeedClients();

                // 6. Seed Demo Users and Roles
                await SeedDemoUsersAndRoles();

                TempData["SuccessMessage"] = "System initialized successfully! Demo data created. You can now login with any demo account.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error initializing system: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private async Task ClearIdentityServerData()
        {
            // Clear all IdentityServer4 data
            _configContext.Clients.RemoveRange(_configContext.Clients);
            _configContext.ApiResources.RemoveRange(_configContext.ApiResources);
            _configContext.ApiScopes.RemoveRange(_configContext.ApiScopes);
            _configContext.IdentityResources.RemoveRange(_configContext.IdentityResources);
            
            _persistedGrantContext.PersistedGrants.RemoveRange(_persistedGrantContext.PersistedGrants);
            
            // Clear demo users (keep system users)
            var demoUsers = await _userManager.Users.Where(u => u.Email.Contains("@demo.com")).ToListAsync();
            foreach (var user in demoUsers)
            {
                await _userManager.DeleteAsync(user);
            }
            
            // Clear demo roles (keep system roles)
            var demoRoles = await _roleManager.Roles.Where(r => r.Name.Contains("Demo") || r.Name == "Admin").ToListAsync();
            foreach (var role in demoRoles)
            {
                await _roleManager.DeleteAsync(role);
            }
            
            await _configContext.SaveChangesAsync();
            await _persistedGrantContext.SaveChangesAsync();
        }

        private async Task SeedIdentityResources()
        {
            // Create Identity Resources directly as entities
            var identityResources = new[]
            {
                        new Duende.IdentityServer.EntityFramework.Entities.IdentityResource
                {
                    Enabled = true,
                    Name = "openid",
                    DisplayName = "Your user identifier",
                    Description = "Your user identifier",
                    Required = true,
                    Emphasize = false,
                    ShowInDiscoveryDocument = true,
                    Created = DateTime.UtcNow,
                    NonEditable = false
                },
                        new Duende.IdentityServer.EntityFramework.Entities.IdentityResource
                {
                    Enabled = true,
                    Name = "profile",
                    DisplayName = "User profile",
                    Description = "Your user profile information (first name, last name, etc.)",
                    Required = false,
                    Emphasize = true,
                    ShowInDiscoveryDocument = true,
                    Created = DateTime.UtcNow,
                    NonEditable = false
                },
                        new Duende.IdentityServer.EntityFramework.Entities.IdentityResource
                {
                    Enabled = true,
                    Name = "email",
                    DisplayName = "Your email address",
                    Description = "Your email address",
                    Required = false,
                    Emphasize = true,
                    ShowInDiscoveryDocument = true,
                    Created = DateTime.UtcNow,
                    NonEditable = false
                },
                        new Duende.IdentityServer.EntityFramework.Entities.IdentityResource
                {
                    Enabled = true,
                    Name = "roles",
                    DisplayName = "Your role(s)",
                    Description = "Your role(s)",
                    Required = false,
                    Emphasize = false,
                    ShowInDiscoveryDocument = true,
                    Created = DateTime.UtcNow,
                    NonEditable = false
                }
            };

            foreach (var resource in identityResources)
            {
                _configContext.IdentityResources.Add(resource);
            }
            await _configContext.SaveChangesAsync();
        }

        private async Task SeedApiScopes()
        {
            var apiScopes = new[]
            {
                        new Duende.IdentityServer.EntityFramework.Entities.ApiScope
                {
                    Enabled = true,
                    Name = "garage.api",
                    DisplayName = "Garage Management API",
                    Description = "Garage Management API",
                    Required = false,
                    Emphasize = false,
                    ShowInDiscoveryDocument = true
                },
                        new Duende.IdentityServer.EntityFramework.Entities.ApiScope
                {
                    Enabled = true,
                    Name = "garage.api.read",
                    DisplayName = "Garage Management API - Read Access",
                    Description = "Read access to garage management data",
                    Required = false,
                    Emphasize = false,
                    ShowInDiscoveryDocument = true
                },
                        new Duende.IdentityServer.EntityFramework.Entities.ApiScope
                {
                    Enabled = true,
                    Name = "garage.api.write",
                    DisplayName = "Garage Management API - Write Access",
                    Description = "Write access to garage management data",
                    Required = false,
                    Emphasize = false,
                    ShowInDiscoveryDocument = true
                },
                        new Duende.IdentityServer.EntityFramework.Entities.ApiScope
                {
                    Enabled = true,
                    Name = "admin.api",
                    DisplayName = "Admin API for management",
                    Description = "Admin API for system management",
                    Required = false,
                    Emphasize = false,
                    ShowInDiscoveryDocument = true
                }
            };

            foreach (var scopeItem in apiScopes)
            {
                _configContext.ApiScopes.Add(scopeItem);
            }
            await _configContext.SaveChangesAsync();
        }

        private async Task SeedApiResources()
        {
                    var apiResources = new[]
                    {
                        new Duende.IdentityServer.EntityFramework.Entities.ApiResource
                {
                    Enabled = true,
                    Name = "garage.api",
                    DisplayName = "Garage Management API",
                    Description = "Garage Management API",
                    AllowedAccessTokenSigningAlgorithms = null,
                    ShowInDiscoveryDocument = true,
                    Created = DateTime.UtcNow,
                    NonEditable = false,
                    Scopes = new List<ApiResourceScope>
                    {
                        new ApiResourceScope { Scope = "garage.api" },
                        new ApiResourceScope { Scope = "garage.api.read" },
                        new ApiResourceScope { Scope = "garage.api.write" },
                        new ApiResourceScope { Scope = "garage.api.admin" }
                    },
                    UserClaims = new List<ApiResourceClaim>
                    {
                        new ApiResourceClaim { Type = "sub" },
                        new ApiResourceClaim { Type = "name" },
                        new ApiResourceClaim { Type = "email" },
                        new ApiResourceClaim { Type = "role" }
                    }
                },
                        new Duende.IdentityServer.EntityFramework.Entities.ApiResource
                {
                    Enabled = true,
                    Name = "admin.api",
                    DisplayName = "Admin API for management",
                    Description = "Admin API for system management",
                    AllowedAccessTokenSigningAlgorithms = null,
                    ShowInDiscoveryDocument = true,
                    Created = DateTime.UtcNow,
                    NonEditable = false,
                    Scopes = new List<ApiResourceScope>
                    {
                        new ApiResourceScope { Scope = "admin.api" }
                    },
                    UserClaims = new List<ApiResourceClaim>
                    {
                        new ApiResourceClaim { Type = "sub" },
                        new ApiResourceClaim { Type = "name" },
                        new ApiResourceClaim { Type = "email" },
                        new ApiResourceClaim { Type = "role" },
                        new ApiResourceClaim { Type = "admin_access" }
                    }
                }
            };

            foreach (var resource in apiResources)
            {
                _configContext.ApiResources.Add(resource);
            }
            await _configContext.SaveChangesAsync();
        }

        private async Task SeedClients()
        {
                    var clients = new[]
                    {
                        new Duende.IdentityServer.EntityFramework.Entities.Client
                {
                    Enabled = true,
                    ClientId = "garage.web",
                    ProtocolType = "oidc",
                    RequireClientSecret = true,
                    ClientName = "Garage Web MVC Client",
                    Description = "Web MVC client for garage management",
                    RequireConsent = false,
                    AllowRememberConsent = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = false, // Tạm thời tắt PKCE để test
                    AllowPlainTextPkce = false,
                    RequireRequestObject = false,
                    AllowAccessTokensViaBrowser = false,
                    AllowOfflineAccess = true,
                    IdentityTokenLifetime = 300,
                    AccessTokenLifetime = 3600,
                    AuthorizationCodeLifetime = 300,
                    AbsoluteRefreshTokenLifetime = 2592000,
                    SlidingRefreshTokenLifetime = 1296000,
                    RefreshTokenUsage = 1, // ReUse
                    UpdateAccessTokenClaimsOnRefresh = false,
                    RefreshTokenExpiration = 1, // Sliding
                    AccessTokenType = 0, // Jwt
                    EnableLocalLogin = true,
                    IncludeJwtId = false,
                    AlwaysSendClientClaims = false,
                    Created = DateTime.UtcNow,
                    NonEditable = false,
                    AllowedGrantTypes = new List<ClientGrantType>
                    {
                        new ClientGrantType { GrantType = "authorization_code" },
                        new ClientGrantType { GrantType = "refresh_token" }
                    },
                    ClientSecrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Value = "garage.web.secret".Sha256(),
                            Type = "SharedSecret",
                            Description = "Garage Web Client Secret",
                            Created = DateTime.UtcNow
                        }
                    },
                    RedirectUris = new List<ClientRedirectUri>
                    {
                        new ClientRedirectUri { RedirectUri = "https://localhost:44352/signin-oidc" },
                        new ClientRedirectUri { RedirectUri = "http://localhost:50107/signin-oidc" }
                    },
                    PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>
                    {
                        new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = "https://localhost:44352/signout-callback-oidc" },
                        new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = "http://localhost:50107/signout-callback-oidc" }
                    },
                    AllowedScopes = new List<ClientScope>
                    {
                        new ClientScope { Scope = "openid" },
                        new ClientScope { Scope = "profile" }
                    },
                    // Claims = new List<Duende.IdentityServer.EntityFramework.Entities.ClientClaim>
                    // {
                    //     // Custom client claims
                    //     new Duende.IdentityServer.EntityFramework.Entities.ClientClaim { Type = "clientname", Value = "abc" },
                    //     new Duende.IdentityServer.EntityFramework.Entities.ClientClaim { Type = "client_type", Value = "web_mvc" },
                    //     new Duende.IdentityServer.EntityFramework.Entities.ClientClaim { Type = "version", Value = "1.0.0" },
                    //     new Duende.IdentityServer.EntityFramework.Entities.ClientClaim { Type = "environment", Value = "development" }
                    // }
                },
                        new Duende.IdentityServer.EntityFramework.Entities.Client
                {
                    Enabled = true,
                    ClientId = "garage.api.client",
                    ProtocolType = "oidc",
                    RequireClientSecret = true,
                    ClientName = "Garage API Client",
                    Description = "API client for garage management",
                    RequireConsent = false,
                    AllowRememberConsent = false,
                    AlwaysIncludeUserClaimsInIdToken = false,
                    RequirePkce = false,
                    AllowPlainTextPkce = false,
                    RequireRequestObject = false,
                    AllowAccessTokensViaBrowser = false,
                    AllowOfflineAccess = false,
                    IdentityTokenLifetime = 300,
                    AccessTokenLifetime = 3600,
                    AuthorizationCodeLifetime = 300,
                    AbsoluteRefreshTokenLifetime = 2592000,
                    SlidingRefreshTokenLifetime = 1296000,
                    RefreshTokenUsage = 1,
                    UpdateAccessTokenClaimsOnRefresh = false,
                    RefreshTokenExpiration = 1,
                    AccessTokenType = 0,
                    EnableLocalLogin = true,
                    IncludeJwtId = false,
                    AlwaysSendClientClaims = false,
                    Created = DateTime.UtcNow,
                    NonEditable = false,
                    AllowedGrantTypes = new List<ClientGrantType>
                    {
                        new ClientGrantType { GrantType = "client_credentials" }
                    },
                    ClientSecrets = new List<ClientSecret>
                    {
                        new ClientSecret
                        {
                            Value = "garage.api.client.secret".Sha256(),
                            Type = "SharedSecret",
                            Description = "Garage API Client Secret",
                            Created = DateTime.UtcNow
                        }
                    },
                    AllowedScopes = new List<ClientScope>
                    {
                        new ClientScope { Scope = "garage.api" },
                        new ClientScope { Scope = "garage.api.read" },
                        new ClientScope { Scope = "garage.api.write" },
                        new ClientScope { Scope = "garage.api.admin" }
                    }
                }
            };

            foreach (var client in clients)
            {
                _configContext.Clients.Add(client);
            }
            await _configContext.SaveChangesAsync();
        }

        private async Task SeedDemoUsersAndRoles()
        {
            // Create demo roles
            var demoRoles = new List<ApplicationRole>
            {
                new ApplicationRole { Name = "SuperAdmin", Description = "Super Administrator with full access" },
                new ApplicationRole { Name = "Admin", Description = "System Administrator" },
                new ApplicationRole { Name = "Manager", Description = "Garage Manager" },
                new ApplicationRole { Name = "Technician", Description = "Garage Technician" },
                new ApplicationRole { Name = "Receptionist", Description = "Front Desk Receptionist" },
                new ApplicationRole { Name = "Customer", Description = "Customer Role" },
                new ApplicationRole { Name = "DemoUser", Description = "Demo User Role" }
            };

            foreach (var role in demoRoles)
            {
                await _roleManager.CreateAsync(role);
            }

            // Create demo users
            var demoUsers = new List<(ApplicationUser user, string password, string role)>
            {
                (new ApplicationUser 
                { 
                    UserName = "superadmin@demo.com", 
                    Email = "superadmin@demo.com", 
                    NormalizedUserName = "SUPERADMIN@DEMO.COM",
                    NormalizedEmail = "SUPERADMIN@DEMO.COM",
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }, "SuperAdmin@123", "SuperAdmin"),
                
                (new ApplicationUser 
                { 
                    UserName = "admin@demo.com", 
                    Email = "admin@demo.com", 
                    NormalizedUserName = "ADMIN@DEMO.COM",
                    NormalizedEmail = "ADMIN@DEMO.COM",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }, "Admin@123", "Admin"),
                
                (new ApplicationUser 
                { 
                    UserName = "manager@demo.com", 
                    Email = "manager@demo.com", 
                    NormalizedUserName = "MANAGER@DEMO.COM",
                    NormalizedEmail = "MANAGER@DEMO.COM",
                    EmailConfirmed = true,
                    FirstName = "John",
                    LastName = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }, "Manager@123", "Manager"),
                
                (new ApplicationUser 
                { 
                    UserName = "technician@demo.com", 
                    Email = "technician@demo.com", 
                    NormalizedUserName = "TECHNICIAN@DEMO.COM",
                    NormalizedEmail = "TECHNICIAN@DEMO.COM",
                    EmailConfirmed = true,
                    FirstName = "Mike",
                    LastName = "Technician",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }, "Tech@123", "Technician"),
                
                (new ApplicationUser 
                { 
                    UserName = "reception@demo.com", 
                    Email = "reception@demo.com", 
                    NormalizedUserName = "RECEPTION@DEMO.COM",
                    NormalizedEmail = "RECEPTION@DEMO.COM",
                    EmailConfirmed = true,
                    FirstName = "Sarah",
                    LastName = "Receptionist",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }, "Reception@123", "Receptionist"),
                
                (new ApplicationUser 
                { 
                    UserName = "customer@demo.com", 
                    Email = "customer@demo.com", 
                    NormalizedUserName = "CUSTOMER@DEMO.COM",
                    NormalizedEmail = "CUSTOMER@DEMO.COM",
                    EmailConfirmed = true,
                    FirstName = "Jane",
                    LastName = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }, "Customer@123", "Customer"),
                
                (new ApplicationUser 
                { 
                    UserName = "demo@demo.com", 
                    Email = "demo@demo.com", 
                    NormalizedUserName = "DEMO@DEMO.COM",
                    NormalizedEmail = "DEMO@DEMO.COM",
                    EmailConfirmed = true,
                    FirstName = "Demo",
                    LastName = "User",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }, "Demo@123", "DemoUser")
            };

            foreach (var (user, password, role) in demoUsers)
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    
                    // Add claims
                    await _userManager.AddClaimsAsync(user, new System.Security.Claims.Claim[]
                    {
                        new System.Security.Claims.Claim("name", user.FullName),
                        new System.Security.Claims.Claim("given_name", user.FirstName ?? ""),
                        new System.Security.Claims.Claim("family_name", user.LastName ?? ""),
                        new System.Security.Claims.Claim("email", user.Email),
                        new System.Security.Claims.Claim("role", role)
                    });
                }
            }
        }

        private string HashSecret(string secret)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
