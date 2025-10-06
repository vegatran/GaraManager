using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace GarageManagementSystem.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Call API to get dashboard statistics
                var response = await _apiService.GetAsync<dynamic>(ApiEndpoints.Dashboard.Statistics);
                
                if (response.Success)
                {
                    var stats = response.Data;
                    
                    // Set viewbag values from API response
                    ViewBag.CustomerCount = stats?.GetProperty("customerCount").GetInt32() ?? 0;
                    ViewBag.VehicleCount = stats?.GetProperty("vehicleCount").GetInt32() ?? 0;
                    ViewBag.ServiceCount = stats?.GetProperty("serviceCount").GetInt32() ?? 0;
                    ViewBag.OrderCount = stats?.GetProperty("orderCount").GetInt32() ?? 0;
                }
                else
                {
                    ViewBag.Error = response.ErrorMessage ?? "Failed to load dashboard statistics from API";
                }

                // Get user information from claims
                ViewBag.UserName = User.FindFirst("name")?.Value ?? User.FindFirst("preferred_username")?.Value;
                ViewBag.UserEmail = User.FindFirst("email")?.Value;
                ViewBag.UserAddress = User.FindFirst("address")?.Value;
                ViewBag.UserRoles = User.FindAll("role").Select(c => c.Value).ToList();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải dữ liệu: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Display user profile and claims information
        /// </summary>
        public IActionResult Profile()
        {
            // ✅ TỰ ĐỘNG LẤY TẤT CẢ CLAIMS - KHÔNG CẦN CHỈ ĐỊNH TỪNG CÁI
            var userInfo = new
            {
                AllClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
            };

            ViewBag.UserInfo = userInfo;
            return View();
        }

        /// <summary>
        /// Test API endpoint that requires authentication
        /// </summary>
        public IActionResult TestApi()
        {
            return View();
        }

        /// <summary>
        /// Logout user from both local app and IdentityServer
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            // Sign out from local cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Sign out from OpenID Connect and redirect to IdentityServer logout
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            
            // Redirect to IdentityServer logout endpoint
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
