using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ApiResourceEntity = Duende.IdentityServer.EntityFramework.Entities.ApiResource;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class ApiResourceManagementController : Controller
    {
        private readonly ConfigurationDbContext _context;

        public ApiResourceManagementController(ConfigurationDbContext context)
        {
            _context = context;
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
                    Scopes = model.Scopes.Select(s => new ApiResourceScope { Scope = s }).ToList(),
                    Secrets = model.Secrets.Select(secret => new ApiResourceSecret { Value = secret.Sha256(), Description = "API Resource Secret" }).ToList()
                };

                _context.ApiResources.Add(resource);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "API Resource created successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
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

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditApiResourceViewModel model)
        {
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

                resource.UserClaims = model.UserClaims.Select(uc => new ApiResourceClaim { Type = uc }).ToList();
                resource.Scopes = model.Scopes.Select(s => new ApiResourceScope { Scope = s }).ToList();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "API Resource updated successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _context.ApiResources.FindAsync(id);
            if (resource == null)
            {
                return Json(new { success = false, message = "API Resource not found" });
            }

            _context.ApiResources.Remove(resource);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "API Resource deleted successfully!" });
        }

        public async Task<IActionResult> GetApiResources()
        {
            var resources = await _context.ApiResources
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

            return Json(resources);
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

        public List<string> Secrets { get; set; } = new List<string>();
    }

    public class EditApiResourceViewModel : CreateApiResourceViewModel
    {
        public int Id { get; set; }
    }
}
