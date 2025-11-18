using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý kho (Warehouse, Zone, Bin) với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("WarehouseManagement")]
    public class WarehouseManagementController : Controller
    {
        private readonly ApiService _apiService;

        public WarehouseManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý kho
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách kho thông qua API
        /// </summary>
        [HttpGet("GetWarehouses")]
        public async Task<IActionResult> GetWarehouses()
        {
            try
            {
                var endpoint = ApiEndpoints.Warehouses.GetAll;
                var response = await _apiService.GetAsync<ApiResponse<List<WarehouseDto>>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new List<WarehouseDto>());
                }
            }
            catch (Exception ex)
            {
                return Json(new List<WarehouseDto>());
            }
        }

        /// <summary>
        /// Lấy chi tiết kho thông qua API
        /// </summary>
        [HttpGet("GetWarehouse/{id}")]
        public async Task<IActionResult> GetWarehouse(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.GetById, id);
                // ✅ SỬA: Gọi GetAsync<WarehouseDto> thay vì GetAsync<ApiResponse<WarehouseDto>>
                // vì ApiService.GetAsync đã tự động unwrap ApiResponse<T>
                var response = await _apiService.GetAsync<WarehouseDto>(endpoint);
                
                // ✅ SỬA: Kiểm tra response.Success, response.Data != null, và response.Data.Id > 0
                if (response.Success && response.Data != null && response.Data.Id > 0)
                {
                    // ✅ SỬA: Trả về WarehouseDto trực tiếp (không wrapped trong ApiResponse)
                    return Json(response.Data);
                }
                else
                {
                    // ✅ THÊM: Log error để debug
                    var errorMessage = response.ErrorMessage ?? response.Message ?? "Không thể lấy dữ liệu kho";
                    if (response.Data == null)
                    {
                        errorMessage = "Không thể lấy dữ liệu kho từ API";
                    }
                    else if (response.Data.Id == 0)
                    {
                        errorMessage = $"ID kho không hợp lệ (ID: {response.Data.Id}). Response từ API có thể không đúng format.";
                    }
                    return NotFound(Json(new { success = false, message = errorMessage }));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, Json(new { success = false, message = $"Lỗi: {ex.Message}" }));
            }
        }

        /// <summary>
        /// Tạo kho mới thông qua API
        /// </summary>
        [HttpPost("CreateWarehouse")]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.Warehouses.Create;
                // ✅ SỬA: Gọi PostAsync<WarehouseDto> thay vì PostAsync<ApiResponse<WarehouseDto>>
                // vì ApiService.PostAsync đã tự động unwrap ApiResponse<T>
                var response = await _apiService.PostAsync<WarehouseDto>(endpoint, dto);
                
                // ✅ SỬA: Kiểm tra response.Success, response.Data != null, và response.Data.Id > 0
                if (response.Success && response.Data != null && response.Data.Id > 0)
                {
                    return Json(new { success = true, data = response.Data, message = "Tạo kho thành công" });
                }
                else
                {
                    // ✅ THÊM: Log error để debug
                    var errorMessage = response.ErrorMessage ?? response.Message ?? "Lỗi khi tạo kho";
                    if (response.Data == null)
                    {
                        errorMessage = "Không thể lấy dữ liệu kho sau khi tạo";
                    }
                    else if (response.Data.Id == 0)
                    {
                        errorMessage = "ID kho không hợp lệ sau khi tạo";
                    }
                    return BadRequest(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật kho thông qua API
        /// </summary>
        [HttpPut("UpdateWarehouse/{id}")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return BadRequest(new { success = false, message = "ID không khớp" });
                }

                var endpoint = string.Format(ApiEndpoints.Warehouses.Update, id);
                var response = await _apiService.PutAsync<ApiResponse<WarehouseDto>>(endpoint, dto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Cập nhật kho thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi cập nhật kho" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa kho thông qua API
        /// </summary>
        [HttpDelete("DeleteWarehouse/{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.Delete, id);
                var response = await _apiService.DeleteAsync<ApiResponse>(endpoint);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa kho thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi xóa kho" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tạo khu vực mới thông qua API
        /// </summary>
        [HttpPost("CreateZone/{warehouseId}")]
        public async Task<IActionResult> CreateZone(int warehouseId, [FromBody] WarehouseZoneRequestDto dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.CreateZone, warehouseId);
                var response = await _apiService.PostAsync<ApiResponse<WarehouseZoneDto>>(endpoint, dto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Tạo khu vực thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi tạo khu vực" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật khu vực thông qua API
        /// </summary>
        [HttpPut("UpdateZone/{warehouseId}/{zoneId}")]
        public async Task<IActionResult> UpdateZone(int warehouseId, int zoneId, [FromBody] WarehouseZoneRequestDto dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.UpdateZone, warehouseId, zoneId);
                var response = await _apiService.PutAsync<ApiResponse<WarehouseZoneDto>>(endpoint, dto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Cập nhật khu vực thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi cập nhật khu vực" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa khu vực thông qua API
        /// </summary>
        [HttpDelete("DeleteZone/{warehouseId}/{zoneId}")]
        public async Task<IActionResult> DeleteZone(int warehouseId, int zoneId)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.DeleteZone, warehouseId, zoneId);
                var response = await _apiService.DeleteAsync<ApiResponse>(endpoint);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa khu vực thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi xóa khu vực" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tạo kệ/ngăn mới thông qua API
        /// </summary>
        [HttpPost("CreateBin/{warehouseId}")]
        public async Task<IActionResult> CreateBin(int warehouseId, [FromBody] WarehouseBinRequestDto dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.CreateBin, warehouseId);
                var response = await _apiService.PostAsync<ApiResponse<WarehouseBinDto>>(endpoint, dto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Tạo kệ/ngăn thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi tạo kệ/ngăn" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật kệ/ngăn thông qua API
        /// </summary>
        [HttpPut("UpdateBin/{warehouseId}/{binId}")]
        public async Task<IActionResult> UpdateBin(int warehouseId, int binId, [FromBody] WarehouseBinRequestDto dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.UpdateBin, warehouseId, binId);
                var response = await _apiService.PutAsync<ApiResponse<WarehouseBinDto>>(endpoint, dto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Cập nhật kệ/ngăn thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi cập nhật kệ/ngăn" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa kệ/ngăn thông qua API
        /// </summary>
        [HttpDelete("DeleteBin/{warehouseId}/{binId}")]
        public async Task<IActionResult> DeleteBin(int warehouseId, int binId)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Warehouses.DeleteBin, warehouseId, binId);
                var response = await _apiService.DeleteAsync<ApiResponse>(endpoint);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa kệ/ngăn thành công" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response.ErrorMessage ?? "Lỗi khi xóa kệ/ngăn" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

