using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý kiểm kê định kỳ với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("InventoryChecks")]
    public class InventoryChecksController : Controller
    {
        private readonly ApiService _apiService;

        public InventoryChecksController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý kiểm kê định kỳ
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách phiếu kiểm kê cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetInventoryChecks")]
        public async Task<IActionResult> GetInventoryChecks(
            int? warehouseId = null,
            int? warehouseZoneId = null,
            int? warehouseBinId = null,
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
                if (warehouseZoneId.HasValue)
                    queryParams.Add($"warehouseZoneId={warehouseZoneId.Value}");
                if (warehouseBinId.HasValue)
                    queryParams.Add($"warehouseBinId={warehouseBinId.Value}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var endpoint = ApiEndpoints.InventoryChecks.GetAll;
                if (queryParams.Any())
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _apiService.GetAsync<ApiResponse<List<InventoryCheckDto>>>(endpoint);
                
                if (response.Success && response.Data != null && response.Data.Data != null)
                {
                    // Convert to DataTables format
                    var dataList = response.Data.Data; // response.Data.Data is List<InventoryCheckDto>?
                    var count = dataList?.Count ?? 0;
                    return Json(new
                    {
                        data = dataList ?? new List<InventoryCheckDto>(),
                        recordsTotal = count,
                        recordsFiltered = count
                    });
                }
                else
                {
                    return Json(new
                    {
                        data = new List<InventoryCheckDto>(),
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        error = response.ErrorMessage ?? "Lỗi khi lấy danh sách phiếu kiểm kê"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = new List<InventoryCheckDto>(),
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách warehouses cho dropdown (với zones và bins)
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

        /// <summary>
        /// Export danh sách phiếu kiểm kê ra Excel
        /// </summary>
        [HttpGet("ExportExcel")]
        public async Task<IActionResult> ExportExcel(
            int? warehouseId = null,
            int? warehouseZoneId = null,
            int? warehouseBinId = null,
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
                if (warehouseZoneId.HasValue)
                    queryParams.Add($"warehouseZoneId={warehouseZoneId.Value}");
                if (warehouseBinId.HasValue)
                    queryParams.Add($"warehouseBinId={warehouseBinId.Value}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var endpoint = ApiEndpoints.InventoryChecks.ExportExcel;
                if (queryParams.Any())
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _apiService.GetByteArrayAsync(endpoint);
                if (!response.Success || response.Data == null)
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không thể xuất Excel." });
                }

                var fileName = string.IsNullOrWhiteSpace(response.Message)
                    ? $"DanhSachKiemKe-{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    : response.Message.Trim('"');

                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Export chi tiết phiếu kiểm kê ra Excel
        /// </summary>
        [HttpGet("ExportDetailExcel/{id}")]
        public async Task<IActionResult> ExportDetailExcel(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.InventoryChecks.ExportDetailExcel.Replace("{0}", id.ToString());
                var response = await _apiService.GetByteArrayAsync(endpoint);
                if (!response.Success || response.Data == null)
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không thể xuất Excel." });
                }

                var fileName = string.IsNullOrWhiteSpace(response.Message)
                    ? $"ChiTietKiemKe-{id}-{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    : response.Message.Trim('"');

                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

