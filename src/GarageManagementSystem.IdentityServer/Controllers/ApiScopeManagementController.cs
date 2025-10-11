using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.IdentityServer.Data; // Added for GaraManagementContext
using ApiScopeEntity = Duende.IdentityServer.EntityFramework.Entities.ApiScope;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class ApiScopeManagementController : Controller
    {
        private readonly ConfigurationDbContext _context;
        private readonly GaraManagementContext _garaContext;
        private readonly IMemoryCache _cache;

        public ApiScopeManagementController(ConfigurationDbContext context, GaraManagementContext garaContext, IMemoryCache cache)
        {
            _context = context;
            _garaContext = garaContext;
            _cache = cache;
        }

        // Helper method to invalidate cache when API scopes are modified
        private void InvalidateApiScopeCache()
        {
            _cache.Remove("available_api_scopes");
            _cache.Remove("all_api_scopes");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateApiScope", new CreateApiScopeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateApiScopeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var scope = new ApiScopeEntity
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Required = model.Required,
                    Emphasize = model.Emphasize,
                    ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                    UserClaims = model.UserClaims.Select(uc => new ApiScopeClaim { Type = uc }).ToList()
                };

                _context.ApiScopes.Add(scope);
                await _context.SaveChangesAsync();

                // Invalidate cache after creating new API scope
                InvalidateApiScopeCache();

                return Json(new { success = true, message = "API Scope created successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var scope = await _context.ApiScopes
                .Include(s => s.UserClaims)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scope == null)
            {
                return NotFound($"API Scope with id '{id}' not found");
            }

            var model = new EditApiScopeViewModel
            {
                Id = scope.Id,
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Required = scope.Required,
                Emphasize = scope.Emphasize,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                UserClaims = scope.UserClaims.Select(uc => uc.Type).ToList()
            };

            return PartialView("_EditApiScope", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditApiScopeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var scope = await _context.ApiScopes
                    .Include(s => s.UserClaims)
                    .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (scope == null)
                {
                    return Json(new { success = false, message = "API Scope not found" });
                }

                scope.Name = model.Name;
                scope.DisplayName = model.DisplayName;
                scope.Description = model.Description;
                scope.Required = model.Required;
                scope.Emphasize = model.Emphasize;
                scope.ShowInDiscoveryDocument = model.ShowInDiscoveryDocument;

                // Update user claims
                _context.RemoveRange(scope.UserClaims);
                scope.UserClaims = model.UserClaims.Select(uc => new ApiScopeClaim { Type = uc }).ToList();

                await _context.SaveChangesAsync();

                // Invalidate cache after updating API scope
                InvalidateApiScopeCache();

                return Json(new { success = true, message = "API Scope updated successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var scope = await _context.ApiScopes
                .Include(s => s.UserClaims)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scope == null)
            {
                return NotFound($"API Scope with id '{id}' not found");
            }

            var viewModel = new
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Required = scope.Required,
                Emphasize = scope.Emphasize,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                UserClaims = scope.UserClaims.Select(uc => uc.Type).ToList()
            };

            return PartialView("_ApiScopeDetails", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var scope = await _context.ApiScopes.FindAsync(id);
            if (scope == null)
            {
                return Json(new { success = false, message = "API Scope not found" });
            }

            // Insert into SoftDeleteRecords using Entity Framework (Soft Delete)
            var softDeleteRecord = new SoftDeleteRecord
            {
                EntityType = "ApiScope",
                EntityName = scope.Name,
                DeletedAt = DateTime.Now,
                DeletedBy = User.Identity?.Name ?? "System",
                Reason = "User requested deletion"
            };

            _garaContext.SoftDeleteRecords.Add(softDeleteRecord);
            await _garaContext.SaveChangesAsync();

            // Invalidate cache after deleting API scope
            InvalidateApiScopeCache();

            return Json(new { success = true, message = "API Scope deleted successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var scope = await _context.ApiScopes.FindAsync(id);
            if (scope == null)
            {
                return Json(new { success = false, message = "API Scope not found" });
            }

            // Remove from SoftDeleteRecords using Entity Framework (Restore)
            var softDeleteRecord = await _garaContext.SoftDeleteRecords
                .FirstOrDefaultAsync(s => s.EntityType == "ApiScope" && s.EntityName == scope.Name);
            
            if (softDeleteRecord != null)
            {
                _garaContext.SoftDeleteRecords.Remove(softDeleteRecord);
                await _garaContext.SaveChangesAsync();
            }

            // Invalidate cache after restoring API scope
            InvalidateApiScopeCache();

            return Json(new { success = true, message = "API Scope restored successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetApiScopes()
        {
            // Get all API Scopes first
            var allScopes = await _context.ApiScopes
                .Include(s => s.UserClaims)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    displayName = s.DisplayName,
                    description = s.Description,
                    required = s.Required,
                    emphasize = s.Emphasize,
                    showInDiscoveryDocument = s.ShowInDiscoveryDocument,
                    userClaims = s.UserClaims.Select(uc => uc.Type).ToList()
                }).ToListAsync();

            // Get soft deleted scope names
            var deletedScopeNames = await _garaContext.SoftDeleteRecords
                .Where(s => s.EntityType == "ApiScope")
                .Select(s => s.EntityName)
                .ToListAsync();

            // Process in memory to add isActive flag
            var processedScopes = allScopes.Select(s => new
            {
                id = s.id,
                name = s.name,
                displayName = s.displayName,
                description = s.description,
                required = s.required,
                emphasize = s.emphasize,
                showInDiscoveryDocument = s.showInDiscoveryDocument,
                userClaims = s.userClaims,
                isActive = !deletedScopeNames.Contains(s.name) // Active if not in deleted list
            }).OrderByDescending(s => s.isActive) // Active scopes first
              .ThenBy(s => s.name)
              .ToList();

            return Json(new { data = processedScopes });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableClaims()
        {
            const string cacheKey = "available_claims_for_api_scope";
            
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
    }

    public class CreateApiScopeViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public bool Required { get; set; } = false;

        public bool Emphasize { get; set; } = false;

        public bool ShowInDiscoveryDocument { get; set; } = true;

        public List<string> UserClaims { get; set; } = new List<string>();
    }

    public class EditApiScopeViewModel : CreateApiScopeViewModel
    {
        public int Id { get; set; }
    }
}
