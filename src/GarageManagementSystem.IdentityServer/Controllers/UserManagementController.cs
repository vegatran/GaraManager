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
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly GaraManagementContext _garaContext;
        private readonly IMemoryCache _cache;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            GaraManagementContext garaContext,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _garaContext = garaContext;
            _cache = cache;
        }

        // Helper method to invalidate cache when users are modified
        private void InvalidateUserCache()
        {
            _cache.Remove("all_users");
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign roles if provided
                    if (model.Roles != null && model.Roles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.Roles);
                    }

                    // Invalidate cache after creating new user
                    InvalidateUserCache();

                    return Json(new { success = true, message = "User created successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create user", errors = result.Errors.Select(e => e.Description) });
                }
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = userRoles.ToList()
            };

            return PartialView("_EditUser", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update user roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(model.Roles);
                    var rolesToAdd = model.Roles.Except(currentRoles);

                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    await _userManager.AddToRolesAsync(user, rolesToAdd);

                    // Invalidate cache after updating user
                    InvalidateUserCache();

                    return Json(new { success = true, message = "User updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update user", errors = result.Errors.Select(e => e.Description) });
                }
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            try
            {
                // Soft delete: Set IsActive to false and record in SoftDeleteRecords
                user.IsActive = false;
                await _userManager.UpdateAsync(user);

                // Record soft delete
                var softDeleteRecord = new SoftDeleteRecord
                {
                    EntityType = "User",
                    EntityName = user.Email,
                    DeletedAt = DateTime.Now,
                    DeletedBy = User.Identity?.Name ?? "System"
                };

                _garaContext.SoftDeleteRecords.Add(softDeleteRecord);
                await _garaContext.SaveChangesAsync();

                // Invalidate cache after soft deleting user
                InvalidateUserCache();

                return Json(new { success = true, message = "User deactivated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to deactivate user", errors = new[] { ex.Message } });
            }
        }

        public async Task<IActionResult> GetUsers()
        {
            const string cacheKey = "all_users";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out var cachedUsers))
            {
                return Json(new { data = cachedUsers });
            }

            var allUsers = await _userManager.Users.ToListAsync();
            var users = new List<object>();

            foreach (var user in allUsers)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                
                users.Add(new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    fullName = user.FirstName + " " + user.LastName,
                    isActive = user.IsActive,
                    createdAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    roles = userRoles
                });
            }

            // Cache for 30 minutes (users don't change often)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.High,
                Size = 1 // Each cache entry counts as 1 unit toward the size limit
            };
            
            _cache.Set(cacheKey, users, cacheOptions);

            return Json(new { data = users });
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = userRoles.ToList()
            };

            return PartialView("_UserDetails", model);
        }

        [HttpPost]
        public async Task<IActionResult> Restore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            try
            {
                // Restore: Set IsActive to true and remove from SoftDeleteRecords
                user.IsActive = true;
                await _userManager.UpdateAsync(user);

                // Remove soft delete record
                var softDeleteRecord = await _garaContext.SoftDeleteRecords
                    .FirstOrDefaultAsync(s => s.EntityType == "User" && s.EntityName == user.Email);

                if (softDeleteRecord != null)
                {
                    _garaContext.SoftDeleteRecords.Remove(softDeleteRecord);
                    await _garaContext.SaveChangesAsync();
                }

                // Invalidate cache after restoring user
                InvalidateUserCache();

                return Json(new { success = true, message = "User restored successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to restore user", errors = new[] { ex.Message } });
            }
        }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;
        
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class EditUserViewModel
    {
        [Required(ErrorMessage = "User ID is required.")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
