using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý nhà cung cấp với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("SupplierManagement")]
    public class SupplierManagementController : Controller
    {
        private readonly ApiService _apiService;

        public SupplierManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý nhà cung cấp
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp cho DataTable thông qua API với pagination
        /// </summary>
        [HttpGet("GetSuppliers")]
        public async Task<IActionResult> GetSuppliers(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var endpoint = ApiEndpoints.Suppliers.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<SupplierDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<SupplierDto>
                    {
                        Data = new List<SupplierDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách nhà cung cấp"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<SupplierDto>
                {
                    Data = new List<SupplierDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhà cung cấp theo ID thông qua API
        /// </summary>
        [HttpGet("GetSupplier/{id}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var response = await _apiService.GetAsync<ApiResponse<SupplierDto>>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Suppliers.GetById, id)
            );
            
            if (response.Success && response.Data != null)
            {
                var supplier = response.Data.Data;
                var supplierData = new
                {
                    id = supplier.Id,
                    supplierCode = supplier.SupplierCode,
                    supplierName = supplier.SupplierName,
                    contactPerson = supplier.ContactPerson,
                    phone = supplier.Phone,
                    email = supplier.Email,
                    address = supplier.Address,
                    isActive = supplier.IsActive
                };
                
                return Json(new ApiResponse { Data = supplierData, Success = true, StatusCode = System.Net.HttpStatusCode.OK });
            }
            
            return Json(new { success = false, error = "Nhà cung cấp không tồn tại" });
        }

        /// <summary>
        /// Tạo nhà cung cấp mới thông qua API
        /// </summary>
        [HttpPost("CreateSupplier")]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto model)
        {
            try
            {
                // Log input data for debugging
                Console.WriteLine($"DEBUG: CreateSupplier input: {System.Text.Json.JsonSerializer.Serialize(model)}");

                if (!ModelState.IsValid)
                {
                    var errors = new Dictionary<string, string[]>();
                    foreach (var key in ModelState.Keys)
                    {
                        var modelErrors = ModelState[key].Errors.Select(e => e.ErrorMessage).ToArray();
                        if (modelErrors.Length > 0)
                        {
                            errors[key] = modelErrors;
                        }
                    }
                    Console.WriteLine($"DEBUG: ModelState errors: {System.Text.Json.JsonSerializer.Serialize(errors)}");
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
                }

                var response = await _apiService.PostAsync<SupplierDto>(ApiEndpoints.Suppliers.Create, model);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Tạo nhà cung cấp thành công" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Lỗi khi tạo nhà cung cấp" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: CreateSupplier exception: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp thông qua API
        /// </summary>
        [HttpPut("UpdateSupplier/{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != model.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<SupplierDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Suppliers.Update, id), 
                model
            );
            
            return Json(response);
        }

        /// <summary>
        /// Xóa nhà cung cấp thông qua API
        /// </summary>
        [HttpDelete("DeleteSupplier/{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var response = await _apiService.DeleteAsync<SupplierDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Suppliers.Delete, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp đang hoạt động cho dropdown
        /// </summary>
        [HttpGet("GetActiveSuppliers")]
        public async Task<IActionResult> GetActiveSuppliers()
        {
            var response = await _apiService.GetAsync<List<SupplierDto>>(ApiEndpoints.Suppliers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var suppliers = response.Data
                    .Where(s => s.IsActive)
                    .Select(s => new
                    {
                        value = s.Id.ToString(),
                        text = s.SupplierName
                    }).Cast<object>().ToList();
                
                return Json(suppliers);
            }

            return Json(new List<object>());
        }
    }
}
