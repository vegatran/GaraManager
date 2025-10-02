using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using GarageManagementSystem.IdentityServer.Data;
using GarageManagementSystem.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class ClaimsManagementController : Controller
    {
        private readonly GaraManagementContext _context;
        private readonly IMemoryCache _cache;

        public ClaimsManagementController(GaraManagementContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Helper method to invalidate cache when claims are modified
        private void InvalidateClaimsCache()
        {
            _cache.Remove("available_claims");
            _cache.Remove("all_claims");
        }

        // GET: ClaimsManagement
        public IActionResult Index()
        {
            return View();
        }

        // GET: ClaimsManagement/GetClaims
        [HttpGet]
        public async Task<IActionResult> GetClaims()
        {
            // Get all claims first
            var allClaims = await _context.Claims
                .OrderBy(c => c.Category)
                .ThenBy(c => c.DisplayName)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    displayName = c.DisplayName,
                    description = c.Description,
                    isStandard = c.IsStandard,
                    category = c.Category,
                    customValueSource = c.CustomValueSource,
                    defaultValue = c.DefaultValue,
                    createdAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                }).ToListAsync();

            // Get soft deleted claim names
            var deletedClaimNames = await _context.SoftDeleteRecords
                .Where(s => s.EntityType == "Claim")
                .Select(s => s.EntityName)
                .ToListAsync();

            // Process in memory to add isActive flag and sort
            var processedClaims = allClaims.Select(c => new
            {
                id = c.id,
                name = c.name,
                displayName = c.displayName,
                description = c.description,
                isActive = !deletedClaimNames.Contains(c.name), // Active if not in deleted list
                isStandard = c.isStandard,
                category = c.category,
                createdAt = c.createdAt
            }).OrderByDescending(c => c.isActive)
              .ThenBy(c => c.category)
              .ThenBy(c => c.displayName)
              .ToList();

            return Json(new { data = processedClaims });
        }

        // GET: ClaimsManagement/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound($"Claim with id '{id}' not found");
            }

            var viewModel = new
            {
                Id = claim.Id,
                Name = claim.Name,
                DisplayName = claim.DisplayName,
                Description = claim.Description,
                IsStandard = claim.IsStandard,
                IsActive = claim.IsActive,
                Category = claim.Category,
                CreatedAt = claim.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                UpdatedAt = claim.UpdatedAt?.ToString("yyyy-MM-dd HH:mm")
            };

            return PartialView("_ClaimDetails", viewModel);
        }

        // GET: ClaimsManagement/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateClaimViewModel();
            return PartialView("_CreateClaim", model);
        }

        // POST: ClaimsManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if claim name already exists
                if (await _context.Claims.AnyAsync(c => c.Name == model.Name))
                {
                    return Json(new { success = false, message = "Claim name already exists" });
                }

                var claim = new Claim
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    IsStandard = model.IsStandard,
                    IsActive = model.IsActive,
                    Category = model.Category,
                    CustomValueSource = model.CustomValueSource,
                    DefaultValue = model.DefaultValue,
                    CreatedAt = DateTime.Now
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                // Invalidate cache after creating new claim
                InvalidateClaimsCache();

                return Json(new { success = true, message = "Claim created successfully" });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed", errorMessages = errors });
        }

        // GET: ClaimsManagement/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound($"Claim with id '{id}' not found");
            }

            var model = new EditClaimViewModel
            {
                Id = claim.Id,
                Name = claim.Name,
                DisplayName = claim.DisplayName,
                Description = claim.Description,
                IsStandard = claim.IsStandard,
                IsActive = claim.IsActive,
                Category = claim.Category,
                CustomValueSource = claim.CustomValueSource,
                DefaultValue = claim.DefaultValue,
                OriginalName = claim.Name // Track original name
            };


            return PartialView("_EditClaim", model);
        }

        // POST: ClaimsManagement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditClaimViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed", errorMessages = validationErrors });
            }
            
            var claim = await _context.Claims.FindAsync(model.Id);
            if (claim == null)
            {
                return Json(new { success = false, message = "Claim not found" });
            }

            // Check if claim name already exists (excluding current claim)
            if (await _context.Claims.AnyAsync(c => c.Name == model.Name && c.Name != model.OriginalName))
            {
                return Json(new { success = false, message = "Claim name already exists" });
            }

            claim.Name = model.Name;
            claim.DisplayName = model.DisplayName;
            claim.Description = model.Description;
            claim.IsStandard = model.IsStandard;
            claim.IsActive = model.IsActive;
            claim.Category = model.Category;
            claim.CustomValueSource = model.CustomValueSource;
            claim.DefaultValue = model.DefaultValue;
            claim.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Invalidate cache after updating claim
            InvalidateClaimsCache();

            return Json(new { success = true, message = "Claim updated successfully" });
        }

        // POST: ClaimsManagement/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return Json(new { success = false, message = "Claim not found" });
            }

            // Insert into SoftDeleteRecords using raw SQL (Soft Delete)
            var sql = @"INSERT INTO SoftDeleteRecords (EntityType, EntityName, DeletedAt, DeletedBy, Reason) 
                       VALUES (@EntityType, @EntityName, @DeletedAt, @DeletedBy, @Reason)";
            
            await _context.Database.ExecuteSqlRawAsync(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@EntityType", "Claim"),
                new Microsoft.Data.SqlClient.SqlParameter("@EntityName", claim.Name),
                new Microsoft.Data.SqlClient.SqlParameter("@DeletedAt", DateTime.Now),
                new Microsoft.Data.SqlClient.SqlParameter("@DeletedBy", User.Identity?.Name ?? "System"),
                new Microsoft.Data.SqlClient.SqlParameter("@Reason", "User requested deletion"));

            // Invalidate cache after deleting claim
            InvalidateClaimsCache();

            return Json(new { success = true, message = "Claim deleted successfully" });
        }

        // POST: ClaimsManagement/Restore/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return Json(new { success = false, message = "Claim not found" });
            }

            // Remove from SoftDeleteRecords (Restore)
            var sql = @"DELETE FROM SoftDeleteRecords 
                       WHERE EntityType = 'Claim' AND EntityName = @EntityName";
            
            await _context.Database.ExecuteSqlRawAsync(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@EntityName", claim.Name));

            // Invalidate cache after restoring claim
            InvalidateClaimsCache();

            return Json(new { success = true, message = "Claim restored successfully" });
        }

        // GET: ClaimsManagement/GetAvailableClaims (for dropdowns)
        [HttpGet]
        public async Task<IActionResult> GetAvailableClaims()
        {
            const string cacheKey = "available_claims";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedClaims))
            {
                return Json(cachedClaims);
            }

            // Get from database if not in cache
            var claims = await _context.Claims
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.DisplayName)
                .Select(c => new { value = c.Name, text = c.DisplayName ?? c.Name })
                .ToListAsync();

            // Cache for 30 minutes (claims don't change often)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1 // Each cache entry counts as 1 unit toward the size limit
            };
            
            _cache.Set(cacheKey, claims, cacheOptions);

            return Json(claims);
        }
    }

    public class CreateClaimViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DisplayName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsStandard { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(200)]
        public string? CustomValueSource { get; set; }

        [StringLength(500)]
        public string? DefaultValue { get; set; }
    }

    public class EditClaimViewModel : CreateClaimViewModel
    {
        public int Id { get; set; }
        public string OriginalName { get; set; } = string.Empty; // Track original name for update
    }
}
