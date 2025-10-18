using GarageManagementSystem.Shared.DTOs;
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
        /// Lấy danh sách tất cả nhà cung cấp cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetSuppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            var response = await _apiService.GetAsync<List<SupplierDto>>(ApiEndpoints.Suppliers.GetAll);
            
            if (response.Success)
            {
                var supplierList = new List<object>();
                
                if (response.Data != null)
                {
                    supplierList = response.Data.Select(s => new
                    {
                        id = s.Id,
                        supplierCode = s.SupplierCode,
                        name = s.SupplierName,
                        contactPerson = s.ContactPerson,
                        phone = s.Phone,
                        email = s.Email,
                        address = s.Address,
                        isActive = s.IsActive,
                        createdAt = s.CreatedAt
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = supplierList,
                    message = "Lấy danh sách nhà cung cấp thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết nhà cung cấp theo ID thông qua API
        /// </summary>
        [HttpGet("GetSupplier/{id}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var response = await _apiService.GetAsync<SupplierDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Suppliers.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo nhà cung cấp mới thông qua API
        /// </summary>
        [HttpPost("CreateSupplier")]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<SupplierDto>(ApiEndpoints.Suppliers.Create, model);
            
            return Json(response);
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
