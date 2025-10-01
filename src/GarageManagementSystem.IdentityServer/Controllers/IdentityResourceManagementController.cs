using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.IdentityServer.Data;
using IdentityResourceEntity = Duende.IdentityServer.EntityFramework.Entities.IdentityResource;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class IdentityResourceManagementController : Controller
    {
        private readonly ConfigurationDbContext _context;
        private readonly GaraManagementContext _garaContext;

        public IdentityResourceManagementController(ConfigurationDbContext context, GaraManagementContext garaContext)
        {
            _context = context;
            _garaContext = garaContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateIdentityResource", new CreateIdentityResourceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateIdentityResourceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resource = new IdentityResourceEntity
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Required = model.Required,
                    Emphasize = model.Emphasize,
                    ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                    UserClaims = model.UserClaims.Select(uc => new IdentityResourceClaim { Type = uc }).ToList()
                };

                _context.IdentityResources.Add(resource);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Identity Resource created successfully!" });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed", errorMessages = errors });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _context.IdentityResources
                .Include(r => r.UserClaims)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                return NotFound($"Identity Resource with id '{id}' not found");
            }

            var model = new EditIdentityResourceViewModel
            {
                Id = resource.Id,
                Name = resource.Name,
                DisplayName = resource.DisplayName,
                Description = resource.Description,
                Required = resource.Required,
                Emphasize = resource.Emphasize,
                ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument,
                UserClaims = resource.UserClaims.Select(uc => uc.Type).ToList()
            };

            return PartialView("_EditIdentityResource", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditIdentityResourceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resource = await _context.IdentityResources
                    .Include(r => r.UserClaims)
                    .FirstOrDefaultAsync(r => r.Id == model.Id);

                if (resource == null)
                {
                    return Json(new { success = false, message = "Identity Resource not found" });
                }

                resource.Name = model.Name;
                resource.DisplayName = model.DisplayName;
                resource.Description = model.Description;
                resource.Required = model.Required;
                resource.Emphasize = model.Emphasize;
                resource.ShowInDiscoveryDocument = model.ShowInDiscoveryDocument;

                // Update user claims
                _context.RemoveRange(resource.UserClaims);
                resource.UserClaims = model.UserClaims.Select(uc => new IdentityResourceClaim { Type = uc }).ToList();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Identity Resource updated successfully!" });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed", errorMessages = errors });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var resource = await _context.IdentityResources
                .Include(r => r.UserClaims)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                return NotFound($"Identity Resource with id '{id}' not found");
            }

            var viewModel = new
            {
                Name = resource.Name,
                DisplayName = resource.DisplayName,
                Description = resource.Description,
                Required = resource.Required,
                Emphasize = resource.Emphasize,
                ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument,
                UserClaims = resource.UserClaims.Select(uc => uc.Type).ToList()
            };

            return PartialView("_IdentityResourceDetails", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _context.IdentityResources.FindAsync(id);
            if (resource == null)
            {
                return Json(new { success = false, message = "Identity Resource not found" });
            }

            // Insert into SoftDeleteRecords using raw SQL (Soft Delete)
            var sql = @"INSERT INTO SoftDeleteRecords (EntityType, EntityName, DeletedAt, DeletedBy, Reason) 
                       VALUES (@EntityType, @EntityName, @DeletedAt, @DeletedBy, @Reason)";
            
            await _context.Database.ExecuteSqlRawAsync(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@EntityType", "IdentityResource"),
                new Microsoft.Data.SqlClient.SqlParameter("@EntityName", resource.Name),
                new Microsoft.Data.SqlClient.SqlParameter("@DeletedAt", DateTime.Now),
                new Microsoft.Data.SqlClient.SqlParameter("@DeletedBy", User.Identity?.Name ?? "System"),
                new Microsoft.Data.SqlClient.SqlParameter("@Reason", "User requested deletion"));

            return Json(new { success = true, message = "Identity Resource deleted successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var resource = await _context.IdentityResources.FindAsync(id);
            if (resource == null)
            {
                return Json(new { success = false, message = "Identity Resource not found" });
            }

            // Remove from SoftDeleteRecords (Restore)
            var sql = @"DELETE FROM SoftDeleteRecords 
                       WHERE EntityType = 'IdentityResource' AND EntityName = @EntityName";
            
            await _context.Database.ExecuteSqlRawAsync(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@EntityName", resource.Name));

            return Json(new { success = true, message = "Identity Resource restored successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetIdentityResources()
        {
            // Get all Identity Resources first
            var allResources = await _context.IdentityResources
                .Include(r => r.UserClaims)
                .Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    displayName = r.DisplayName,
                    description = r.Description,
                    required = r.Required,
                    emphasize = r.Emphasize,
                    showInDiscoveryDocument = r.ShowInDiscoveryDocument,
                    userClaims = r.UserClaims.Select(uc => uc.Type).ToList()
                }).ToListAsync();

            // Get soft deleted resource names
            var deletedResourceNames = await _garaContext.SoftDeleteRecords
                .Where(s => s.EntityType == "IdentityResource")
                .Select(s => s.EntityName)
                .ToListAsync();

            // Process in memory to add isActive flag
            var processedResources = allResources.Select(r => new
            {
                id = r.id,
                name = r.name,
                displayName = r.displayName,
                description = r.description,
                required = r.required,
                emphasize = r.emphasize,
                showInDiscoveryDocument = r.showInDiscoveryDocument,
                userClaims = r.userClaims,
                isActive = !deletedResourceNames.Contains(r.name) // Active if not in deleted list
            }).OrderByDescending(r => r.isActive) // Active resources first
              .ThenBy(r => r.name)
              .ToList();

            return Json(new { data = processedResources });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableClaims()
        {
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

            return Json(activeClaims);
        }
    }

    public class CreateIdentityResourceViewModel
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

    public class EditIdentityResourceViewModel : CreateIdentityResourceViewModel
    {
        public int Id { get; set; }
    }
}
