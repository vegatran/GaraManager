using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý điều chỉnh tồn kho với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("InventoryAdjustments")]
    public class InventoryAdjustmentsController : Controller
    {
        private readonly ApiService _apiService;

        public InventoryAdjustmentsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý điều chỉnh tồn kho
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách phiếu điều chỉnh cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetInventoryAdjustments")]
        public async Task<IActionResult> GetInventoryAdjustments(
            int? warehouseId = null,
            int? inventoryCheckId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>();

                if (warehouseId.HasValue)
                    queryParams.Add($"warehouseId={warehouseId.Value}");
                if (inventoryCheckId.HasValue)
                    queryParams.Add($"inventoryCheckId={inventoryCheckId.Value}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var endpoint = ApiEndpoints.InventoryAdjustments.GetAll;
                if (queryParams.Any())
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _apiService.GetAsync<ApiResponse<List<InventoryAdjustmentDto>>>(endpoint);
                
                if (response.Success && response.Data != null && response.Data.Data != null)
                {
                    // Convert to DataTables format
                    var dataList = response.Data.Data;
                    var count = dataList?.Count ?? 0;
                    return Json(new
                    {
                        data = dataList ?? new List<InventoryAdjustmentDto>(),
                        recordsTotal = count,
                        recordsFiltered = count
                    });
                }
                else
                {
                    return Json(new
                    {
                        data = new List<InventoryAdjustmentDto>(),
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        error = response.ErrorMessage ?? "Lỗi khi lấy danh sách phiếu điều chỉnh"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = new List<InventoryAdjustmentDto>(),
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách warehouses cho dropdown
        /// </summary>
        [HttpGet("Warehouses")]
        public async Task<IActionResult> GetWarehouses()
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<WarehouseDto>>>(ApiEndpoints.Warehouses.GetAll);
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data });
                }
                return Json(new { success = false, data = new List<WarehouseDto>() });
            }
            catch
            {
                return Json(new { success = false, data = new List<WarehouseDto>() });
            }
        }
    }
}

