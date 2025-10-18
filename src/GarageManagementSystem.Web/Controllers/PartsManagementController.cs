using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý phụ tùng với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("PartsManagement")]
    public class PartsManagementController : Controller
    {
        private readonly ApiService _apiService;

        public PartsManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý phụ tùng
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả phụ tùng cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetParts")]
        public async Task<IActionResult> GetParts()
        {
            var response = await _apiService.GetAsync<List<PartDto>>(ApiEndpoints.Parts.GetAll);
            
            if (response.Success)
            {
                var partList = new List<object>();
                
                if (response.Data != null)
                {
                    partList = response.Data.Select(p => new
                    {
                        id = p.Id,
                        partNumber = p.PartNumber,
                        name = p.PartName,
                        description = p.Description ?? "N/A",
                        category = p.Category ?? "N/A",
                        brand = p.Brand ?? "N/A",
                        price = p.SellPrice.ToString("N0"),
                        stockQuantity = p.QuantityInStock.ToString(),
                        minStockLevel = p.MinimumStock.ToString(),
                        maxStockLevel = p.ReorderLevel?.ToString() ?? "0",
                        unit = p.Unit ?? "N/A",
                        location = p.Location ?? "N/A",
                        status = p.IsActive ? "Active" : "Inactive",
                        createdDate = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = partList,
                    message = "Lấy danh sách phụ tùng thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết phụ tùng theo ID thông qua API
        /// </summary>
        [HttpGet("GetPart/{id}")]
        public async Task<IActionResult> GetPart(int id)
        {
            var response = await _apiService.GetAsync<PartDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Parts.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo phụ tùng mới thông qua API
        /// </summary>
        [HttpPost("CreatePart")]
        public async Task<IActionResult> CreatePart([FromBody] CreatePartDto partDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<PartDto>(
                ApiEndpoints.Parts.Create,
                partDto
            );

            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin phụ tùng thông qua API
        /// </summary>
        [HttpPut("UpdatePart/{id}")]
        public async Task<IActionResult> UpdatePart(int id, [FromBody] UpdatePartDto partDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != partDto.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<PartDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Parts.Update, id),
                partDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa phụ tùng thông qua API
        /// </summary>
        [HttpDelete("DeletePart/{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            var response = await _apiService.DeleteAsync<PartDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.Parts.Delete, id)
            );

            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableSuppliers")]
        public async Task<IActionResult> GetAvailableSuppliers()
        {
            var response = await _apiService.GetAsync<List<SupplierDto>>(ApiEndpoints.Suppliers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var suppliers = response.Data.Select(s => new
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
