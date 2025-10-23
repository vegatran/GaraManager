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
                    ViewBag.CustomerCount = stats?.GetProperty("Total")?.GetProperty("Customers").GetInt32() ?? 0;
                    ViewBag.VehicleCount = stats?.GetProperty("Total")?.GetProperty("Vehicles").GetInt32() ?? 0;
                    ViewBag.ServiceCount = stats?.GetProperty("ThisMonth")?.GetProperty("NewOrders").GetInt32() ?? 0;
                    ViewBag.OrderCount = stats?.GetProperty("Total")?.GetProperty("ActiveOrders").GetInt32() ?? 0;
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
            var identityServerAuthority = _configuration["IdentityServer:Authority"] ?? "https://ids.ladtechs.com";
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


        /// <summary>
        /// Get dashboard statistics for JavaScript
        /// </summary>
        [Route("GetDashboardStatistics")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                var response = await _apiService.GetAsync<dynamic>(ApiEndpoints.Dashboard.Statistics);
                
                if (response.Success)
                {
                    var stats = response.Data;
                    
                    // Safe property access with try-catch for each property
                    var dashboardData = new
                    {
                        totalCustomers = GetPropertyValue(stats, "Total", "Customers", 0),
                        totalVehicles = GetPropertyValue(stats, "Total", "Vehicles", 0),
                        pendingOrders = GetPropertyValue(stats, "Total", "ActiveOrders", 0),
                        todayAppointments = GetPropertyValue(stats, "Today", "Appointments", 0),
                        monthlyRevenue = GetPropertyValue(stats, "ThisMonth", "Revenue", 0m),
                        lastMonthRevenue = GetPropertyValue(stats, "LastMonth", "Revenue", 0m),
                        completedOrders = GetPropertyValue(stats, "OrderStatus", "Completed", 0),
                        inProgressOrders = GetPropertyValue(stats, "OrderStatus", "InProgress", 0),
                        newCustomers = GetPropertyValue(stats, "ThisMonth", "NewCustomers", 0),
                        newOrders = GetPropertyValue(stats, "ThisMonth", "NewOrders", 0)
                    };
                    
                    return Json(new { success = true, data = dashboardData });
                }
                
                return Json(new { success = false, message = "Không thể tải thống kê" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private T GetPropertyValue<T>(dynamic stats, string parentKey, string childKey, T defaultValue)
        {
            try
            {
                if (stats == null) return defaultValue;
                
                var parent = stats.GetProperty(parentKey);
                if (parent == null) return defaultValue;
                
                var child = parent.GetProperty(childKey);
                if (child == null) return defaultValue;
                
                if (typeof(T) == typeof(int))
                    return (T)(object)child.GetInt32();
                else if (typeof(T) == typeof(decimal))
                    return (T)(object)child.GetDecimal();
                else if (typeof(T) == typeof(string))
                    return (T)(object)child.GetString();
                
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get recent orders for dashboard
        /// </summary>
        [Route("GetRecentOrders")]
        public async Task<IActionResult> GetRecentOrders()
        {
            try
            {
                // Tạm thời trả về dữ liệu mẫu
                var recentOrders = new[]
                {
                    new { orderNumber = "ORD001", customerName = "Nguyễn Văn A", orderDate = "2024-01-15", totalAmount = 1500000, status = "Hoàn thành", statusClass = "success" },
                    new { orderNumber = "ORD002", customerName = "Trần Thị B", orderDate = "2024-01-14", totalAmount = 2300000, status = "Đang xử lý", statusClass = "warning" },
                    new { orderNumber = "ORD003", customerName = "Lê Văn C", orderDate = "2024-01-13", totalAmount = 1800000, status = "Chờ duyệt", statusClass = "info" }
                };
                
                return Json(new { success = true, data = recentOrders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Get upcoming appointments for dashboard
        /// </summary>
        [Route("GetUpcomingAppointments")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            try
            {
                // Tạm thời trả về dữ liệu mẫu
                var upcomingAppointments = new[]
                {
                    new { customerName = "Nguyễn Văn A", vehicleLicensePlate = "30A-12345", appointmentDate = "2024-01-16", appointmentTime = "09:00", serviceType = "Bảo dưỡng định kỳ" },
                    new { customerName = "Trần Thị B", vehicleLicensePlate = "51B-67890", appointmentDate = "2024-01-16", appointmentTime = "14:00", serviceType = "Sửa chữa động cơ" },
                    new { customerName = "Lê Văn C", vehicleLicensePlate = "43C-11111", appointmentDate = "2024-01-17", appointmentTime = "10:30", serviceType = "Thay phụ tùng" }
                };
                
                return Json(new { success = true, data = upcomingAppointments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Get low stock alerts for dashboard
        /// </summary>
        [Route("GetLowStockAlerts")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            try
            {
                var response = await _apiService.GetAsync<dynamic>("inventory-alerts/low-stock");
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }
                
                return Json(new { success = false, message = "Không thể tải cảnh báo tồn kho thấp" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Get out of stock alerts for dashboard
        /// </summary>
        [Route("GetOutOfStockAlerts")]
        public async Task<IActionResult> GetOutOfStockAlerts()
        {
            try
            {
                var response = await _apiService.GetAsync<dynamic>("inventory-alerts/out-of-stock");
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }
                
                return Json(new { success = false, message = "Không thể tải cảnh báo hết hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// Get reorder suggestions for dashboard
        /// </summary>
        [Route("GetReorderSuggestions")]
        public async Task<IActionResult> GetReorderSuggestions()
        {
            try
            {
                var response = await _apiService.GetAsync<dynamic>("inventory-alerts/reorder-suggestions");
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data });
                }
                
                return Json(new { success = false, message = "Không thể tải gợi ý đặt hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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
