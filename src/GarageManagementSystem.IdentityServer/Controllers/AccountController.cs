using GarageManagementSystem.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            Console.WriteLine($"DEBUG: Login attempt - Email: {model.Email}");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"DEBUG: ModelState is valid");
                
                // Check if user exists
                var user = await _userManager.FindByEmailAsync(model.Email);
                Console.WriteLine($"DEBUG: User found: {user != null}");
                
                if (user == null)
                {
                    Console.WriteLine($"DEBUG: User not found for email: {model.Email}");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
                
                Console.WriteLine($"DEBUG: User ID: {user.Id}");
                Console.WriteLine($"DEBUG: User Name: {user.UserName}");
                Console.WriteLine($"DEBUG: Email: {user.Email}");
                Console.WriteLine($"DEBUG: Normalized User Name: {user.NormalizedUserName}");
                Console.WriteLine($"DEBUG: Email Confirmed: {user.EmailConfirmed}");
                Console.WriteLine($"DEBUG: Lockout Enabled: {user.LockoutEnabled}");
                Console.WriteLine($"DEBUG: IsActive: {user.IsActive}");
                
                // Test password
                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
                Console.WriteLine($"DEBUG: Password Check: {passwordCheck}");

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: false);

                Console.WriteLine($"DEBUG: SignIn Result - Succeeded: {result.Succeeded}");
                Console.WriteLine($"DEBUG: SignIn Result - IsLockedOut: {result.IsLockedOut}");
                Console.WriteLine($"DEBUG: SignIn Result - IsNotAllowed: {result.IsNotAllowed}");
                Console.WriteLine($"DEBUG: SignIn Result - RequiresTwoFactor: {result.RequiresTwoFactor}");

                if (result.Succeeded)
                {
                    Console.WriteLine($"DEBUG: Login successful, checking authentication context...");
                    Console.WriteLine($"DEBUG: HttpContext.User.Identity.IsAuthenticated: {HttpContext.User.Identity?.IsAuthenticated}");
                    Console.WriteLine($"DEBUG: HttpContext.User.Identity.Name: {HttpContext.User.Identity?.Name}");
                    Console.WriteLine($"DEBUG: HttpContext.User.Identity.AuthenticationType: {HttpContext.User.Identity?.AuthenticationType}");
                    
                    // Force authentication context refresh
                    await _signInManager.RefreshSignInAsync(user);
                    
                    Console.WriteLine($"DEBUG: After RefreshSignIn - HttpContext.User.Identity.IsAuthenticated: {HttpContext.User.Identity?.IsAuthenticated}");
                    Console.WriteLine($"DEBUG: After RefreshSignIn - HttpContext.User.Identity.Name: {HttpContext.User.Identity?.Name}");
                    
                    Console.WriteLine($"DEBUG: Login successful, redirecting...");
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
                    
                    Console.WriteLine($"DEBUG: Login failed - {errorMsg}");
                    ModelState.AddModelError(string.Empty, errorMsg);
                }
            }
            else
            {
                Console.WriteLine("DEBUG: ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"DEBUG: ModelState Error - {error.ErrorMessage}");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
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