using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ClientEntity = Duende.IdentityServer.EntityFramework.Entities.Client;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class ClientManagementController : Controller
    {
        private readonly ConfigurationDbContext _context;

        public ClientManagementController(ConfigurationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            try
            {
                var clients = await _context.Clients
                    .Include(c => c.AllowedGrantTypes)
                    .Include(c => c.AllowedScopes)
                    .Include(c => c.RedirectUris)
                    .Include(c => c.PostLogoutRedirectUris)
                    .Select(c => new
                    {
                        id = c.Id,
                        clientId = c.ClientId,
                        clientName = c.ClientName,
                        description = c.Description ?? "",
                        allowedGrantTypes = c.AllowedGrantTypes.Select(gt => gt.GrantType).ToList(),
                        allowedScopes = c.AllowedScopes.Select(s => s.Scope).ToList(),
                        redirectUris = c.RedirectUris.Select(u => u.RedirectUri).ToList(),
                        postLogoutRedirectUris = c.PostLogoutRedirectUris.Select(u => u.PostLogoutRedirectUri).ToList(),
                        enabled = c.Enabled,
                        requireClientSecret = c.RequireClientSecret,
                        requirePkce = c.RequirePkce,
                        allowOfflineAccess = c.AllowOfflineAccess,
                        allowRememberConsent = c.AllowRememberConsent,
                        requireConsent = c.RequireConsent,
                        accessTokenLifetime = c.AccessTokenLifetime,
                        identityTokenLifetime = c.IdentityTokenLifetime,
                        created = c.Created
                    })
                    .ToListAsync();

                // Return data in DataTables expected format
                return Json(new { data = clients });
            }
            catch (Exception ex)
            {
                return Json(new { data = new object[0], error = ex.Message });
            }
        }

        public IActionResult Create()
        {
            return View(new CreateClientViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = new ClientEntity
                {
                    ClientId = model.ClientId,
                    ClientName = model.ClientName,
                    Description = model.Description,
                    
                    // Basic Security Settings
                    Enabled = model.Enabled,
                    RequireClientSecret = model.RequireClientSecret,
                    RequirePkce = model.RequirePkce,
                    AllowOfflineAccess = model.AllowOfflineAccess,
                    AllowRememberConsent = model.AllowRememberConsent,
                    RequireConsent = model.RequireConsent,
                    
                    // Token Settings
                    AccessTokenLifetime = model.AccessTokenLifetime,
                    IdentityTokenLifetime = model.IdentityTokenLifetime,
                    AuthorizationCodeLifetime = model.AuthorizationCodeLifetime,
                    UserSsoLifetime = model.UserSsoLifetime,
                    ConsentLifetime = model.ConsentLifetime ?? 0,
                    
                    // Logout Settings
                    FrontChannelLogoutUri = model.FrontChannelLogoutUri,
                    FrontChannelLogoutSessionRequired = model.FrontChannelLogoutSessionRequired,
                    BackChannelLogoutUri = model.BackChannelLogoutUri,
                    BackChannelLogoutSessionRequired = model.BackChannelLogoutSessionRequired,
                    
                    // Token Encryption
                    RequireRequestObject = model.RequireRequestObject,
                    
                    // Client Claims Management
                    AlwaysSendClientClaims = model.AlwaysSendClientClaims,
                    ClientClaimsPrefix = model.ClientClaimsPrefix,
                    
                    // Additional Client Properties
                    ProtocolType = model.ProtocolType,
                    IncludeJwtId = model.IncludeJwtId,
                    AllowAccessTokensViaBrowser = model.AllowAccessTokensViaBrowser,
                    UpdateAccessTokenClaimsOnRefresh = model.UpdateAccessTokenClaimsOnRefresh,
                    // IncludeUserClaimsInAccessToken = model.IncludeUserClaimsInAccessToken, // May not exist in older versions
                    
                    // Device Flow (these properties may not exist in older versions)
                    // AllowDeviceFlow = model.AllowDeviceFlow,
                    // DeviceCodeLifetime = model.DeviceCodeLifetime,
                    
                    // Collections
                    ClientSecrets = !string.IsNullOrEmpty(model.ClientSecret) 
                        ? new List<ClientSecret> { new ClientSecret { Value = model.ClientSecret.Sha256() } }
                        : new List<ClientSecret>(),
                    
                    // Use backward compatibility properties if new ones are empty
                    AllowedGrantTypes = (model.AllowedGrantTypes?.Any() == true ? model.AllowedGrantTypes : model.GrantTypes)
                        .Select(gt => new ClientGrantType { GrantType = gt }).ToList(),
                    AllowedScopes = (model.AllowedScopes?.Any() == true ? model.AllowedScopes : model.Scopes)
                        .Select(s => new ClientScope { Scope = s }).ToList(),
                    RedirectUris = model.RedirectUris.Select(ru => new ClientRedirectUri { RedirectUri = ru }).ToList(),
                    PostLogoutRedirectUris = model.PostLogoutRedirectUris.Select(pru => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = pru }).ToList(),
                    AllowedCorsOrigins = model.AllowedCorsOrigins.Select(co => new ClientCorsOrigin { Origin = co }).ToList(),
                    Claims = model.ClientClaims.Select(cc => new Duende.IdentityServer.EntityFramework.Entities.ClientClaim { Type = cc.Type, Value = cc.Value }).ToList()
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Client created successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.Claims)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new CreateClientViewModel
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                Description = client.Description,
                
                // Basic Security Settings
                Enabled = client.Enabled,
                RequireClientSecret = client.RequireClientSecret,
                RequirePkce = client.RequirePkce,
                AllowOfflineAccess = client.AllowOfflineAccess,
                AllowRememberConsent = client.AllowRememberConsent,
                RequireConsent = client.RequireConsent,
                
                // Token Settings
                AccessTokenLifetime = client.AccessTokenLifetime,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                UserSsoLifetime = client.UserSsoLifetime ?? 2592000,
                ConsentLifetime = client.ConsentLifetime ?? 0,
                
                // Logout Settings
                FrontChannelLogoutUri = client.FrontChannelLogoutUri,
                FrontChannelLogoutSessionRequired = client.FrontChannelLogoutSessionRequired,
                BackChannelLogoutUri = client.BackChannelLogoutUri,
                BackChannelLogoutSessionRequired = client.BackChannelLogoutSessionRequired,
                
                // Advanced Settings
                RequireRequestObject = client.RequireRequestObject,
                // AllowDeviceFlow = client.AllowDeviceFlow, // May not exist in older versions
                // DeviceCodeLifetime = client.DeviceCodeLifetime, // May not exist in older versions
                
                // Client Claims Management
                AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                ClientClaimsPrefix = client.ClientClaimsPrefix,
                
                // Additional Client Properties
                ProtocolType = client.ProtocolType,
                IncludeJwtId = client.IncludeJwtId,
                AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
                // IncludeUserClaimsInAccessToken = client.IncludeUserClaimsInAccessToken, // May not exist in older versions
                
                // Collections
                GrantTypes = client.AllowedGrantTypes.Select(gt => gt.GrantType).ToList(),
                Scopes = client.AllowedScopes.Select(s => s.Scope).ToList(),
                RedirectUris = client.RedirectUris.Select(ru => ru.RedirectUri).ToList(),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(pru => pru.PostLogoutRedirectUri).ToList(),
                AllowedCorsOrigins = client.AllowedCorsOrigins.Select(co => co.Origin).ToList(),
                ClientClaims = client.Claims.Select(cc => new ClientClaimViewModel { Id = cc.Id, Type = cc.Type, Value = cc.Value }).ToList()
            };

            ViewBag.ClientId = id;
            return PartialView("_EditClient", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateClientViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                var client = await _context.Clients
                    .Include(c => c.AllowedGrantTypes)
                    .Include(c => c.AllowedScopes)
                    .Include(c => c.RedirectUris)
                    .Include(c => c.PostLogoutRedirectUris)
                    .Include(c => c.AllowedCorsOrigins)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }

                // Don't update ClientId as it's read-only
                client.ClientName = model.ClientName;
                client.Description = model.Description;
                
                // Basic Security Settings
                client.Enabled = model.Enabled;
                client.RequireClientSecret = model.RequireClientSecret;
                client.RequirePkce = model.RequirePkce;
                client.AllowOfflineAccess = model.AllowOfflineAccess;
                client.AllowRememberConsent = model.AllowRememberConsent;
                client.RequireConsent = model.RequireConsent;
                
                // Token Settings
                client.AccessTokenLifetime = model.AccessTokenLifetime;
                client.IdentityTokenLifetime = model.IdentityTokenLifetime;
                client.AuthorizationCodeLifetime = model.AuthorizationCodeLifetime;
                client.UserSsoLifetime = model.UserSsoLifetime;
                client.ConsentLifetime = model.ConsentLifetime;
                
                // Logout Settings
                client.FrontChannelLogoutUri = model.FrontChannelLogoutUri;
                client.FrontChannelLogoutSessionRequired = model.FrontChannelLogoutSessionRequired;
                client.BackChannelLogoutUri = model.BackChannelLogoutUri;
                client.BackChannelLogoutSessionRequired = model.BackChannelLogoutSessionRequired;
                
                // Token Encryption
                client.RequireRequestObject = model.RequireRequestObject;
                
                // Device Flow (these properties may not exist in older versions)
                // client.AllowDeviceFlow = model.AllowDeviceFlow;
                // client.DeviceCodeLifetime = model.DeviceCodeLifetime;

                // Update Client Secret if provided
                if (!string.IsNullOrEmpty(model.ClientSecret))
                {
                    var existingSecrets = _context.Set<ClientSecret>().Where(cs => cs.ClientId == id).ToList();
                    _context.Set<ClientSecret>().RemoveRange(existingSecrets);
                    client.ClientSecrets = new List<ClientSecret> { new ClientSecret { Value = model.ClientSecret.Sha256() } };
                }

                // Update collections
                var existingGrantTypes = _context.Set<ClientGrantType>().Where(cgt => cgt.ClientId == id).ToList();
                var existingScopes = _context.Set<ClientScope>().Where(cs => cs.ClientId == id).ToList();
                var existingRedirectUris = _context.Set<ClientRedirectUri>().Where(cru => cru.ClientId == id).ToList();
                var existingPostLogoutUris = _context.Set<ClientPostLogoutRedirectUri>().Where(cpru => cpru.ClientId == id).ToList();
                var existingCorsOrigins = _context.Set<ClientCorsOrigin>().Where(cco => cco.ClientId == id).ToList();
                
                _context.Set<ClientGrantType>().RemoveRange(existingGrantTypes);
                _context.Set<ClientScope>().RemoveRange(existingScopes);
                _context.Set<ClientRedirectUri>().RemoveRange(existingRedirectUris);
                _context.Set<ClientPostLogoutRedirectUri>().RemoveRange(existingPostLogoutUris);
                _context.Set<ClientCorsOrigin>().RemoveRange(existingCorsOrigins);

                // Use backward compatibility properties if new ones are empty
                client.AllowedGrantTypes = (model.AllowedGrantTypes?.Any() == true ? model.AllowedGrantTypes : model.GrantTypes)
                    .Select(gt => new ClientGrantType { GrantType = gt }).ToList();
                client.AllowedScopes = (model.AllowedScopes?.Any() == true ? model.AllowedScopes : model.Scopes)
                    .Select(s => new ClientScope { Scope = s }).ToList();
                client.RedirectUris = model.RedirectUris.Select(ru => new ClientRedirectUri { RedirectUri = ru }).ToList();
                client.PostLogoutRedirectUris = model.PostLogoutRedirectUris.Select(pru => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = pru }).ToList();
                client.AllowedCorsOrigins = model.AllowedCorsOrigins.Select(co => new ClientCorsOrigin { Origin = co }).ToList();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Client updated successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return Json(new { success = false, message = "Client not found" });
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Client deleted successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.Claims)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                Description = client.Description,
                Enabled = client.Enabled,
                RequireClientSecret = client.RequireClientSecret,
                RequirePkce = client.RequirePkce,
                AllowOfflineAccess = client.AllowOfflineAccess,
                AllowRememberConsent = client.AllowRememberConsent,
                RequireConsent = client.RequireConsent,
                
                // Token Settings
                AccessTokenLifetime = client.AccessTokenLifetime,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                UserSsoLifetime = client.UserSsoLifetime ?? 2592000,
                ConsentLifetime = client.ConsentLifetime ?? 0,
                
                // Logout Settings
                FrontChannelLogoutUri = client.FrontChannelLogoutUri,
                FrontChannelLogoutSessionRequired = client.FrontChannelLogoutSessionRequired,
                BackChannelLogoutUri = client.BackChannelLogoutUri,
                BackChannelLogoutSessionRequired = client.BackChannelLogoutSessionRequired,
                
                // Advanced Settings
                RequireRequestObject = client.RequireRequestObject,
                // AllowDeviceFlow = client.AllowDeviceFlow, // May not exist in older versions
                // DeviceCodeLifetime = client.DeviceCodeLifetime, // May not exist in older versions
                
                // Client Claims Management
                AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                ClientClaimsPrefix = client.ClientClaimsPrefix,
                
                // Additional Client Properties
                ProtocolType = client.ProtocolType,
                IncludeJwtId = client.IncludeJwtId,
                AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
                // IncludeUserClaimsInAccessToken = client.IncludeUserClaimsInAccessToken, // May not exist in older versions
                
                // Collections
                GrantTypes = client.AllowedGrantTypes.Select(gt => gt.GrantType).ToList(),
                Scopes = client.AllowedScopes.Select(s => s.Scope).ToList(),
                RedirectUris = client.RedirectUris.Select(ru => ru.RedirectUri).ToList(),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(pru => pru.PostLogoutRedirectUri).ToList(),
                CorsOrigins = client.AllowedCorsOrigins.Select(co => co.Origin).ToList(),
                ClientClaims = client.Claims.Select(cc => new { Type = cc.Type, Value = cc.Value }).ToList(),
                
                Created = client.Created
            };

            return PartialView("_ClientDetails", viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> GetAvailableScopes()
        {
            try
            {
                var scopes = await _context.ApiScopes
                    .Where(s => s.Enabled)
                    .Select(s => new { value = s.Name, text = s.DisplayName ?? s.Name })
                    .ToListAsync();

                // Add default OpenID Connect scopes
                var defaultScopes = new[]
                {
                    new { value = "openid", text = "OpenID" },
                    new { value = "profile", text = "Profile" },
                    new { value = "email", text = "Email" },
                    new { value = "address", text = "Address" },
                    new { value = "phone", text = "Phone" },
                    new { value = "roles", text = "Roles" }
                };

                var allScopes = defaultScopes.Concat(scopes).ToList();
                return Json(allScopes);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableGrantTypes()
        {
            try
            {
                var grantTypes = new[]
                {
                    new { value = "authorization_code", text = "Authorization Code" },
                    new { value = "client_credentials", text = "Client Credentials" },
                    new { value = "implicit", text = "Implicit" },
                    new { value = "hybrid", text = "Hybrid" },
                    new { value = "password", text = "Resource Owner Password" },
                    new { value = "refresh_token", text = "Refresh Token" }
                };

                return Json(grantTypes);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

    }

    public class CreateClientViewModel
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        public string ClientName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ClientSecret { get; set; }

        // Basic Security Settings
        public bool Enabled { get; set; } = true;
        public bool RequireClientSecret { get; set; } = true;
        public bool RequirePkce { get; set; } = true;
        public bool AllowOfflineAccess { get; set; } = false;
        public bool AllowRememberConsent { get; set; } = true;
        public bool RequireConsent { get; set; } = false;

        // Token Settings
        public int AccessTokenLifetime { get; set; } = 3600;
        public int IdentityTokenLifetime { get; set; } = 300;
        public int AuthorizationCodeLifetime { get; set; } = 300;
        public int UserSsoLifetime { get; set; } = 2592000; // 30 days
        public int? ConsentLifetime { get; set; }

        // Authentication Methods
        public List<string> AllowedCorsOrigins { get; set; } = new List<string>();
        public List<string> AllowedGrantTypes { get; set; } = new List<string>();
        public List<string> AllowedScopes { get; set; } = new List<string>();
        public List<string> RedirectUris { get; set; } = new List<string>();
        public List<string> PostLogoutRedirectUris { get; set; } = new List<string>();

        // Logout URIs
        public string? FrontChannelLogoutUri { get; set; }
        public bool FrontChannelLogoutSessionRequired { get; set; } = true;
        public string? BackChannelLogoutUri { get; set; }
        public bool BackChannelLogoutSessionRequired { get; set; } = true;

        // Token Encryption
        public bool RequireRequestObject { get; set; } = false;

        // Device Flow
        public bool AllowDeviceFlow { get; set; } = false;
        public int DeviceCodeLifetime { get; set; } = 300;

        // Client Claims Management
        public bool AlwaysSendClientClaims { get; set; } = false;
        public string? ClientClaimsPrefix { get; set; }
        public List<ClientClaimViewModel> ClientClaims { get; set; } = new List<ClientClaimViewModel>();

        // Additional Client Properties
        public string ProtocolType { get; set; } = "oidc";
        public bool IncludeJwtId { get; set; } = false;
        public bool AllowAccessTokensViaBrowser { get; set; } = false;
        public bool UpdateAccessTokenClaimsOnRefresh { get; set; } = false;
        public bool IncludeUserClaimsInAccessToken { get; set; } = true;

        // Backward compatibility properties
        public List<string> GrantTypes { get; set; } = new List<string>(); // Maps to AllowedGrantTypes
        public List<string> Scopes { get; set; } = new List<string>(); // Maps to AllowedScopes
    }

    public class EditClientViewModel : CreateClientViewModel
    {
        public int Id { get; set; }
    }

    public class ClientClaimViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
