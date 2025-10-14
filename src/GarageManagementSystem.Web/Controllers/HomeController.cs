using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    [Authorize]
    [Route("")]
    [Route("Home")]
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;

        public HomeController(ApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        [Authorize]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Gọi API để lấy thống kê tổng quan
                var response = await _apiService.GetAsync<dynamic>(ApiEndpoints.Dashboard.Statistics);
                
                if (response.Success)
                {
                    var stats = response.Data;
                    
                    // Thiết lập giá trị viewbag từ phản hồi API
                    ViewBag.CustomerCount = stats?.GetProperty("customerCount").GetInt32() ?? 0;
                    ViewBag.VehicleCount = stats?.GetProperty("vehicleCount").GetInt32() ?? 0;
                    ViewBag.ServiceCount = stats?.GetProperty("serviceCount").GetInt32() ?? 0;
                    ViewBag.OrderCount = stats?.GetProperty("orderCount").GetInt32() ?? 0;
                }
                else
                {
                    ViewBag.Error = response.ErrorMessage ?? "Không thể tải thống kê tổng quan từ API";
                }

                // Lấy thông tin người dùng từ claims
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
        [Route("Profile")]
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
        [Route("TestApi")]
        public IActionResult TestApi()
        {
            return View();
        }


        /// <summary>
        /// Kiểm tra xử lý token timeout
        /// </summary>
        [Route("TestTokenTimeout")]
        public IActionResult TestTokenTimeout()
        {
            return View();
        }

        /// <summary>
        /// Gỡ lỗi thông tin token
        /// </summary>
        [Route("DebugToken")]
        public async Task<IActionResult> DebugToken()
        {
            var debugInfo = new
            {
                UserIsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserName = User.Identity?.Name ?? "Unknown",
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                AccessTokenFromClaim = User.FindFirst("access_token")?.Value,
                AccessTokenFromContext = await HttpContext.GetTokenAsync("access_token"),
                AllTokens = await HttpContext.GetTokenAsync("access_token"),
                IdToken = await HttpContext.GetTokenAsync("id_token"),
                RefreshToken = await HttpContext.GetTokenAsync("refresh_token")
            };

            return Json(debugInfo);
        }

        /// <summary>
        /// Test routing - simple text response
        /// </summary>
        [Route("TestRouting")]
        public IActionResult TestRouting()
        {
            return Content("Routing works! No redirects!");
        }


        /// <summary>
        /// Lấy cấu hình client cho JavaScript
        /// </summary>
        [Route("GetConfig")]
        public IActionResult GetConfig()
        {
            var identityServerAuthority = _configuration["IdentityServer:Authority"] ?? "https://localhost:44333";
            var apiBaseUrl = _configuration["ApiConfiguration:BaseUrl"] ?? "https://localhost:44303/api/";
            
            var config = new
            {
                IdentityServerAuthority = identityServerAuthority,
                ApiBaseUrl = apiBaseUrl
            };

            Console.WriteLine($"🔧 GetConfig returning: {System.Text.Json.JsonSerializer.Serialize(config)}");
            return Json(config);
        }

        /// <summary>
        /// Login page - redirect to IdentityServer with returnUrl
        /// </summary>
        [AllowAnonymous]
        [Route("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu có returnUrl, truyền vào AuthenticationProperties
            if (!string.IsNullOrEmpty(returnUrl))
            {
                var props = new AuthenticationProperties
                {
                    RedirectUri = returnUrl
                };
                return Challenge(props, "oidc");
            }
            
            return Challenge("oidc");
        }

        /// <summary>
        /// Access denied page
        /// </summary>
        [AllowAnonymous]
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /// <summary>
        /// Logout user from both local app and IdentityServer
        /// </summary>
        [AllowAnonymous]
        [Route("Logout")]
        public IActionResult Logout()
        {
            Console.WriteLine($"🔍 Web App Logout - HomeController.Logout() called");
            Console.WriteLine($"🔍 Web App Logout - Request URL: {Request.GetDisplayUrl()}");
            
            // Đăng xuất cả Cookies và OIDC
            // post_logout_redirect_uri sẽ được set trong Program.cs event handler
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, "oidc");
        }


        [AllowAnonymous]
        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
