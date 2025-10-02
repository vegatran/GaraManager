using GarageManagementSystem.IdentityServer.Models;
using GarageManagementSystem.IdentityServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly GaraManagementContext _garaContext;
        private readonly IMemoryCache _cache;

        public RoleManagementController(
            RoleManager<ApplicationRole> roleManager,
            GaraManagementContext garaContext,
            IMemoryCache cache)
        {
            _roleManager = roleManager;
            _garaContext = garaContext;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Helper method to invalidate cache when roles are modified
        private void InvalidateRoleCache()
        {
            _cache.Remove("all_roles");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var role = new ApplicationRole
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    IsActive = model.IsActive
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    // Record creation in SoftDeleteRecords
                    var createRecord = new SoftDeleteRecord
                    {
                        EntityType = "Role",
                        EntityName = role.Name,
                        DeletedAt = DateTime.Now,
                        DeletedBy = User.Identity?.Name ?? "System",
                        Reason = "Role Created"
                    };

                    _garaContext.SoftDeleteRecords.Add(createRecord);
                    await _garaContext.SaveChangesAsync();

                    // Invalidate cache after creating role
                    InvalidateRoleCache();

                    return Json(new { success = true, message = "Role created successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create role", errors = result.Errors.Select(e => e.Description) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while creating the role", errors = new[] { ex.Message } });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive
            };

            return PartialView("_EditRole", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    return Json(new { success = false, message = "Role not found" });
                }

                role.Name = model.Name;
                role.Description = model.Description;
                role.IsActive = model.IsActive;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    // Invalidate cache after updating role
                    InvalidateRoleCache();

                    return Json(new { success = true, message = "Role updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update role", errors = result.Errors.Select(e => e.Description) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the role", errors = new[] { ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Role not found" });
            }

            try
            {
                // Soft delete: Set IsActive to false and record in SoftDeleteRecords
                role.IsActive = false;
                await _roleManager.UpdateAsync(role);

                // Record soft delete
                var softDeleteRecord = new SoftDeleteRecord
                {
                    EntityType = "Role",
                    EntityName = role.Name,
                    DeletedAt = DateTime.Now,
                    DeletedBy = User.Identity?.Name ?? "System"
                };

                _garaContext.SoftDeleteRecords.Add(softDeleteRecord);
                await _garaContext.SaveChangesAsync();

                // Invalidate cache after soft deleting role
                InvalidateRoleCache();

                return Json(new { success = true, message = "Role deactivated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to deactivate role", errors = new[] { ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Restore(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Role not found" });
            }

            try
            {
                // Restore: Set IsActive to true and remove from SoftDeleteRecords
                role.IsActive = true;
                await _roleManager.UpdateAsync(role);

                // Remove soft delete record
                var softDeleteRecord = await _garaContext.SoftDeleteRecords
                    .FirstOrDefaultAsync(s => s.EntityType == "Role" && s.EntityName == role.Name);

                if (softDeleteRecord != null)
                {
                    _garaContext.SoftDeleteRecords.Remove(softDeleteRecord);
                    await _garaContext.SaveChangesAsync();
                }

                // Invalidate cache after restoring role
                InvalidateRoleCache();

                return Json(new { success = true, message = "Role restored successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to restore role", errors = new[] { ex.Message } });
            }
        }

        public async Task<IActionResult> GetRoles()
        {
            const string cacheKey = "all_roles";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedRoles))
            {
                return Json(new { data = cachedRoles });
            }

            var allRoles = await _roleManager.Roles.ToListAsync();
            var roles = new List<object>();

            foreach (var role in allRoles)
            {
                roles.Add(new
                {
                    id = role.Id,
                    name = role.Name,
                    description = role.Description,
                    isActive = role.IsActive,
                    createdAt = role.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                });
            }

            // Cache for 30 minutes (roles don't change often)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1 // Each cache entry counts as 1 unit toward the size limit
            };
            
            _cache.Set(cacheKey, roles, cacheOptions);

            return Json(new { data = roles });
        }

        public async Task<IActionResult> Details(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new RoleDetailsViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt
            };

            return PartialView("_RoleDetails", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableRoles()
        {
            const string cacheKey = "available_roles";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedRoles))
            {
                return Json(cachedRoles);
            }

            var roles = await _roleManager.Roles
                .Where(r => r.IsActive)
                .Select(r => new { value = r.Name, text = r.Name + " - " + r.Description })
                .ToListAsync();

            // Cache for 30 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1
            };
            
            _cache.Set(cacheKey, roles, cacheOptions);

            return Json(roles);
        }
    }

    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class EditRoleViewModel
    {
        [Required(ErrorMessage = "Role ID is required.")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class RoleDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
