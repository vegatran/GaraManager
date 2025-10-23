using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Web.Controllers
{
    public class InventoryAlertsController : Controller
    {
        private readonly ApiService _apiService;

        public InventoryAlertsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: InventoryAlerts
        public IActionResult Index()
        {
            return View();
        }

        // GET: InventoryAlerts/GetAlerts
        [HttpGet]
        public async Task<IActionResult> GetAlerts(int pageNumber = 1, int pageSize = 10, string alertType = "", string severity = "")
        {
            try
            {
                var queryParams = new List<string>();
                queryParams.Add($"pageNumber={pageNumber}");
                queryParams.Add($"pageSize={pageSize}");
                
                if (!string.IsNullOrEmpty(alertType))
                    queryParams.Add($"alertType={Uri.EscapeDataString(alertType)}");
                    
                if (!string.IsNullOrEmpty(severity))
                    queryParams.Add($"severity={Uri.EscapeDataString(severity)}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"/api/InventoryAlerts?{queryString}";

                var response = await _apiService.GetAsync<PagedResponse<InventoryAlertDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(new
                    {
                        draw = Request.Query["draw"].FirstOrDefault(),
                        recordsTotal = response.Data.TotalCount,
                        recordsFiltered = response.Data.TotalCount,
                        data = response.Data.Data
                    });
                }

                return Json(new
                {
                    draw = Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inventory alerts: {ex.Message}");
                return Json(new
                {
                    draw = Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>()
                });
            }
        }

        // GET: InventoryAlerts/GetAlertTypes
        [HttpGet]
        public async Task<IActionResult> GetAlertTypes()
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<string>>>("/api/InventoryAlerts/alert-types");
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data.Data);
                }
                
                return Json(new List<string>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting alert types: {ex.Message}");
                return Json(new List<string>());
            }
        }

        // POST: InventoryAlerts/MarkAsResolved
        [HttpPost]
        public async Task<IActionResult> MarkAsResolved(int alertId)
        {
            try
            {
                var response = await _apiService.PostAsync<ApiResponse<bool>>($"/api/InventoryAlerts/{alertId}/resolve", null);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Đã đánh dấu cảnh báo là đã xử lý" });
                }
                
                return Json(new { success = false, message = response.Message ?? "Không thể đánh dấu cảnh báo" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking alert as resolved: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi xử lý cảnh báo" });
            }
        }

        // POST: InventoryAlerts/MarkAllAsResolved
        [HttpPost]
        public async Task<IActionResult> MarkAllAsResolved(string alertType = "")
        {
            try
            {
                var endpoint = "/api/InventoryAlerts/resolve-all";
                if (!string.IsNullOrEmpty(alertType))
                {
                    endpoint += $"?alertType={Uri.EscapeDataString(alertType)}";
                }

                var response = await _apiService.PostAsync<ApiResponse<int>>(endpoint, null);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = $"Đã đánh dấu {response.Data} cảnh báo là đã xử lý" });
                }
                
                return Json(new { success = false, message = response.Message ?? "Không thể đánh dấu cảnh báo" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking all alerts as resolved: {ex.Message}");
                return Json(new { success = false, message = "Lỗi khi xử lý cảnh báo" });
            }
        }
    }
}
