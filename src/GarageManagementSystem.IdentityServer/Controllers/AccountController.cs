using GarageManagementSystem.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Http.Extensions;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;


            if (ModelState.IsValid)
            {
                
                // Check if user exists
                var user = await _userManager.FindByEmailAsync(model.Email);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
                
                
                // Test password
                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: false);


                if (result.Succeeded)
                {
                    
                    // Force authentication context refresh
                    await _signInManager.RefreshSignInAsync(user);
                    
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    var errorMsg = "Invalid login attempt.";
                    if (result.IsLockedOut) errorMsg = "Account locked out.";
                    else if (result.IsNotAllowed) errorMsg = "Account not allowed.";
                    else if (result.RequiresTwoFactor) errorMsg = "Two-factor required.";
                    
                    ModelState.AddModelError(string.Empty, errorMsg);
                }
            }
            else
            {
            }

            return View(model);
        }

        // Handle GET logout - IdentityServer4 sẽ redirect về đây
        [HttpGet]
        public async Task<IActionResult> Logout(string? logoutId = null)
        {
            // Sign out user
            await _signInManager.SignOutAsync();
            // Nếu có logoutId, sử dụng IIdentityServerInteractionService.GetLogoutContextAsync để lấy thông tin
            if (!string.IsNullOrEmpty(logoutId))
            {
                // Lấy thông tin về yêu cầu logout từ logoutId
                var logoutContext = await _interaction.GetLogoutContextAsync(logoutId);
                if (logoutContext != null)
                {   
                    var postLogoutRedirectUri = logoutContext.PostLogoutRedirectUri;
                    if (!string.IsNullOrEmpty(postLogoutRedirectUri))
                    {
                        // Redirect về Web App
                        return Redirect(postLogoutRedirectUri);
                    }
                    else
                    {
                        Console.WriteLine($"❌ PostLogoutRedirectUri is null or empty");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ LogoutContext is null");
                }
            }
            // Fallback: redirect về Home/Index của IdentityServer
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}