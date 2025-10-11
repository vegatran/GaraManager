using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.IdentityServer.Data;
using ApiResourceEntity = Duende.IdentityServer.EntityFramework.Entities.ApiResource;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class ApiResourceManagementController : Controller
    {
        private readonly ConfigurationDbContext _context;
        private readonly GaraManagementContext _garaContext;
        private readonly IMemoryCache _cache;

        public ApiResourceManagementController(ConfigurationDbContext context, GaraManagementContext garaContext, IMemoryCache cache)
        {
            _context = context;
            _garaContext = garaContext;
            _cache = cache;
        }

        // Helper method to invalidate cache when API Resources are modified
        private void InvalidateApiResourceCache()
        {
            _cache.Remove("available_api_resources");
            _cache.Remove("available_claims_for_api_resource");
            _cache.Remove("available_scopes_for_api_resource");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View(new CreateApiResourceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateApiResourceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resource = new ApiResourceEntity
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Enabled = model.Enabled,
                    ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                    UserClaims = model.UserClaims.Select(uc => new ApiResourceClaim { Type = uc }).ToList(),
                    Scopes = model.Scopes.Select(s => new ApiResourceScope { Scope = s }).ToList()
                };

                try
                {
                    _context.ApiResources.Add(resource);
                    await _context.SaveChangesAsync();

                    // Invalidate cache after creating new API Resource
                    InvalidateApiResourceCache();

                    return Json(new { success = true, message = "API Resource created successfully!" });
                }
                catch (DbUpdateException ex)
                {
                    // Handle database errors
                    var errorMessage = $"Database error: {ex.InnerException?.Message ?? ex.Message}";
                    
                    return Json(new { 
                        success = false, 
                        message = errorMessage
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"An error occurred: {ex.Message}",
                        exception = ex.ToString()
                    });
                }
            }

            // ModelState validation errors
            var modelErrors = ModelState.Keys.SelectMany(key => 
                ModelState[key].Errors.Select(error => new { Field = key, Error = error.ErrorMessage })
            ).ToList();

            return Json(new { 
                success = false, 
                message = "Invalid model data", 
                modelErrors = modelErrors,
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _context.ApiResources
                .Include(r => r.UserClaims)
                .Include(r => r.Scopes)
                .Include(r => r.Secrets)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                return NotFound();
            }

            var model = new EditApiResourceViewModel
            {
                Id = resource.Id,
                Name = resource.Name,
                DisplayName = resource.DisplayName,
                Description = resource.Description,
                Enabled = resource.Enabled,
                ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument,
                UserClaims = resource.UserClaims.Select(uc => uc.Type).ToList(),
                Scopes = resource.Scopes.Select(s => s.Scope).ToList()
            };

            return PartialView("_EditApiResource", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditApiResourceViewModel model)
        {
            try
            {
                // Log incoming model data for debugging
                var modelData = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Enabled = model.Enabled,
                    ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                    UserClaimsCount = model.UserClaims?.Count ?? 0,
                    ScopesCount = model.Scopes?.Count ?? 0,
                    UserClaims = model.UserClaims,
                    Scopes = model.Scopes
                };

                if (ModelState.IsValid)
                {
                    var resource = await _context.ApiResources
                        .Include(r => r.UserClaims)
                        .Include(r => r.Scopes)
                        .Include(r => r.Secrets)
                        .FirstOrDefaultAsync(r => r.Id == model.Id);

                    if (resource == null)
                    {
                        return Json(new { success = false, message = "API Resource not found" });
                    }

                    resource.Name = model.Name;
                    resource.DisplayName = model.DisplayName;
                    resource.Description = model.Description;
                    resource.Enabled = model.Enabled;
                    resource.ShowInDiscoveryDocument = model.ShowInDiscoveryDocument;

                    // Update collections
                    _context.RemoveRange(resource.UserClaims);
                    _context.RemoveRange(resource.Scopes);

                    resource.UserClaims = model.UserClaims?.Select(uc => new ApiResourceClaim { Type = uc }).ToList() ?? new List<ApiResourceClaim>();
                    resource.Scopes = model.Scopes?.Select(s => new ApiResourceScope { Scope = s }).ToList() ?? new List<ApiResourceScope>();

                    await _context.SaveChangesAsync();

                    // Invalidate cache after updating API Resource
                    InvalidateApiResourceCache();

                    return Json(new { success = true, message = "API Resource updated successfully!" });
                }

                // Log ModelState errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var modelStateErrors = ModelState.Keys.SelectMany(key => ModelState[key].Errors.Select(x => new { Field = key, Error = x.ErrorMessage })).ToList();
                
                return Json(new { 
                    success = false, 
                    message = "Invalid model", 
                    errors = errors,
                    modelStateErrors = modelStateErrors,
                    modelData = modelData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}", stackTrace = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _context.ApiResources.FindAsync(id);
            if (resource == null)
            {
                return Json(new { success = false, message = "API Resource not found" });
            }

            // Insert into SoftDeleteRecords using Entity Framework (Soft Delete)
            var softDeleteRecord = new SoftDeleteRecord
            {
                EntityType = "ApiResource",
                EntityName = resource.Name,
                DeletedAt = DateTime.Now,
                DeletedBy = User.Identity?.Name ?? "System",
                Reason = "User requested deletion"
            };

            _garaContext.SoftDeleteRecords.Add(softDeleteRecord);
            await _garaContext.SaveChangesAsync();

            // Invalidate cache after deleting API Resource
            InvalidateApiResourceCache();

            return Json(new { success = true, message = "API Resource deleted successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var resource = await _context.ApiResources.FindAsync(id);
            if (resource == null)
            {
                return Json(new { success = false, message = "API Resource not found" });
            }

            // Remove from SoftDeleteRecords using Entity Framework (Restore)
            var softDeleteRecord = await _garaContext.SoftDeleteRecords
                .FirstOrDefaultAsync(s => s.EntityType == "ApiResource" && s.EntityName == resource.Name);
            
            if (softDeleteRecord != null)
            {
                _garaContext.SoftDeleteRecords.Remove(softDeleteRecord);
                await _garaContext.SaveChangesAsync();
            }

            // Invalidate cache after restoring API Resource
            InvalidateApiResourceCache();

            return Json(new { success = true, message = "API Resource restored successfully!" });
        }

        public async Task<IActionResult> Details(int id)
        {
            var resource = await _context.ApiResources
                .Include(r => r.UserClaims)
                .Include(r => r.Scopes)
                .Include(r => r.Secrets)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                return NotFound($"API Resource with id '{id}' not found");
            }

            var viewModel = new
            {
                Name = resource.Name,
                DisplayName = resource.DisplayName,
                Description = resource.Description,
                Enabled = resource.Enabled,
                ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument,
                UserClaims = resource.UserClaims.Select(uc => uc.Type).ToList(),
                Scopes = resource.Scopes.Select(s => s.Scope).ToList(),
                Secrets = resource.Secrets.Select(s => new { s.Id, s.Description }).ToList()
            };

            return PartialView("_ApiResourceDetails", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableScopes()
        {
            const string cacheKey = "available_scopes_for_api_resource";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedScopes))
            {
                return Json(cachedScopes);
            }

            // Get all API Scopes first
            var allScopes = await _context.ApiScopes
                .Select(s => new { s.Name, s.DisplayName })
                .ToListAsync();

            // Get soft deleted scope names
            var deletedScopeNames = await _garaContext.SoftDeleteRecords
                .Where(s => s.EntityType == "ApiScope")
                .Select(s => s.EntityName)
                .ToListAsync();

            // Filter out soft deleted scopes
            var activeScopes = allScopes
                .Where(s => !deletedScopeNames.Contains(s.Name))
                .Select(s => new { value = s.Name, text = s.DisplayName ?? s.Name })
                .ToList();

            // Cache for 30 minutes (scopes don't change often)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1 // Each cache entry counts as 1 unit toward the size limit
            };
            
            _cache.Set(cacheKey, activeScopes, cacheOptions);

            return Json(activeScopes);
        }

        [HttpGet]
        public async Task<IActionResult> GetApiScopes()
        {
            const string cacheKey = "available_api_scopes";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedScopes))
            {
                return Json(cachedScopes);
            }

            // Get all API Scopes first
            var allScopes = await _context.ApiScopes
                .Select(s => new { s.Name, s.DisplayName })
                .ToListAsync();

            // Get soft deleted scope names
            var deletedScopeNames = await _garaContext.SoftDeleteRecords
                .Where(s => s.EntityType == "ApiScope")
                .Select(s => s.EntityName)
                .ToListAsync();

            // Filter out soft deleted scopes
            var activeScopes = allScopes
                .Where(s => !deletedScopeNames.Contains(s.Name))
                .Select(s => new { value = s.Name, text = s.DisplayName ?? s.Name })
                .ToList();

            // Cache for 30 minutes (scopes don't change often)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1 // Each cache entry counts as 1 unit toward the size limit
            };
            
            _cache.Set(cacheKey, activeScopes, cacheOptions);

            return Json(activeScopes);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableClaims()
        {
            const string cacheKey = "available_claims_for_api_resource";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedClaims))
            {
                return Json(cachedClaims);
            }

            // Get all claims first
            var allClaims = await _garaContext.Claims
                .OrderBy(c => c.Category)
                .ThenBy(c => c.DisplayName)
                .Select(c => new { c.Name, c.DisplayName })
                .ToListAsync();

            // Get soft deleted claim names
            var deletedClaimNames = await _garaContext.SoftDeleteRecords
                .Where(s => s.EntityType == "Claim")
                .Select(s => s.EntityName)
                .ToListAsync();

            // Filter out soft deleted claims
            var activeClaims = allClaims
                .Where(c => !deletedClaimNames.Contains(c.Name))
                .Select(c => new { value = c.Name, text = c.DisplayName ?? c.Name })
                .ToList();

            // Cache for 30 minutes (claims don't change often)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1 // Each cache entry counts as 1 unit toward the size limit
            };
            
            _cache.Set(cacheKey, activeClaims, cacheOptions);

            return Json(activeClaims);
        }

        [HttpGet]
        public async Task<IActionResult> GetApiResources()
        {
            const string cacheKey = "available_api_resources";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedResources))
            {
                return Json(new { data = cachedResources });
            }

            // Get all API Resources first
            var allResources = await _context.ApiResources
                .Include(r => r.UserClaims)
                .Include(r => r.Scopes)
                .Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    displayName = r.DisplayName,
                    description = r.Description,
                    enabled = r.Enabled,
                    showInDiscoveryDocument = r.ShowInDiscoveryDocument,
                    userClaims = r.UserClaims.Select(uc => uc.Type).ToList(),
                    scopes = r.Scopes.Select(s => s.Scope).ToList()
                }).ToListAsync();

            // Get soft deleted resource names
            var deletedResourceNames = await _garaContext.SoftDeleteRecords
                .Where(s => s.EntityType == "ApiResource")
                .Select(s => s.EntityName)
                .ToListAsync();

            // Process in memory to add isActive flag
            var processedResources = allResources.Select(r => new
            {
                id = r.id,
                name = r.name,
                displayName = r.displayName,
                description = r.description,
                enabled = r.enabled,
                showInDiscoveryDocument = r.showInDiscoveryDocument,
                userClaims = r.userClaims,
                scopes = r.scopes,
                isActive = !deletedResourceNames.Contains(r.name) // Active if not in deleted list
            }).OrderByDescending(r => r.isActive) // Active resources first
              .ThenBy(r => r.name)
              .ToList();

            // Cache for 30 minutes (API Resources don't change often)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1 // Each cache entry counts as 1 unit toward the size limit
            };
            
            _cache.Set(cacheKey, processedResources, cacheOptions);

            return Json(new { data = processedResources });
        }
    }

    public class CreateApiResourceViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public bool Enabled { get; set; } = true;

        public bool ShowInDiscoveryDocument { get; set; } = true;

        public List<string> UserClaims { get; set; } = new List<string>();

        public List<string> Scopes { get; set; } = new List<string>();
    }

    public class EditApiResourceViewModel : CreateApiResourceViewModel
    {
        public int Id { get; set; }
    }
}
