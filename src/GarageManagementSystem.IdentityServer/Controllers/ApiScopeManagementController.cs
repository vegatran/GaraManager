using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ApiScopeEntity = Duende.IdentityServer.EntityFramework.Entities.ApiScope;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class ApiScopeManagementController : Controller
    {
        private readonly ConfigurationDbContext _context;

        public ApiScopeManagementController(ConfigurationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View(new CreateApiScopeViewModel());
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

                return Json(new { success = true, message = "API Scope created successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var scope = await _context.ApiScopes
                .Include(s => s.UserClaims)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scope == null)
            {
                return NotFound();
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

            return View(model);
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

                return Json(new { success = true, message = "API Scope updated successfully!" });
            }

            return Json(new { success = false, message = "Invalid model", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var scope = await _context.ApiScopes.FindAsync(id);
            if (scope == null)
            {
                return Json(new { success = false, message = "API Scope not found" });
            }

            _context.ApiScopes.Remove(scope);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "API Scope deleted successfully!" });
        }

        public async Task<IActionResult> GetApiScopes()
        {
            var scopes = await _context.ApiScopes
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

            return Json(scopes);
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
