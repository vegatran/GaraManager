using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.IdentityServer.Models;

namespace GarageManagementSystem.IdentityServer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine($"DEBUG: Home/Index - User.Identity.IsAuthenticated: {User.Identity.IsAuthenticated}");
            Console.WriteLine($"DEBUG: Home/Index - User.Identity.Name: {User.Identity.Name}");
            Console.WriteLine($"DEBUG: Home/Index - User.Identity.AuthenticationType: {User.Identity.AuthenticationType}");
            
            // Check if any users exist in the system
            var userCount = _userManager.Users.Count();
            if (userCount == 0)
            {
                Console.WriteLine($"DEBUG: Home/Index - No users found, redirecting to setup");
                return RedirectToAction("Index", "Setup");
            }
            
            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine($"DEBUG: Home/Index - User not authenticated, redirecting to login");
                return RedirectToAction("Login", "Account");
            }
            
            Console.WriteLine($"DEBUG: Home/Index - User authenticated, returning view");
            return View();
        }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
