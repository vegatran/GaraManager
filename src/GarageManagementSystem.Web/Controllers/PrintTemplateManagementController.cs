using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý Print Templates với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("PrintTemplateManagement")]
    public class PrintTemplateManagementController : Controller
    {
        private readonly ApiService _apiService;

        public PrintTemplateManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý print templates
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả mẫu in cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetTemplates")]
        public async Task<IActionResult> GetTemplates()
        {
            var response = await _apiService.GetAsync<List<PrintTemplateDto>>(ApiEndpoints.PrintTemplates.GetAll);
            
            if (response.Success)
            {
                var templateList = new List<object>();
                
                if (response.Data != null)
                {
                    templateList = response.Data.Select(t => new
                    {
                        id = t.Id,
                        templateName = t.TemplateName,
                        templateType = t.TemplateType,
                        description = t.Description,
                        isActive = t.IsActive,
                        isDefault = t.IsDefault,
                        createdAt = t.CreatedAt
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = templateList,
                    message = "Lấy danh sách mẫu in thành công"
                });
            }
            
            return Json(new { 
                success = false,
                data = new List<object>(),
                message = response.ErrorMessage ?? "Lỗi khi lấy danh sách mẫu in"
            });
        }

        /// <summary>
        /// Tạo template mặc định cho báo giá
        /// </summary>
        [HttpPost("CreateDefaultQuotation")]
        public async Task<IActionResult> CreateDefaultQuotation()
        {
            try
            {
                var response = await _apiService.PostAsync<bool>(ApiEndpoints.PrintTemplates.CreateDefaultQuotation, new { });
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Tạo mẫu mặc định cho báo giá thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Lỗi khi tạo mẫu mặc định" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tạo mẫu mặc định: {ex.Message}" });
            }
        }

        /// <summary>
        /// Đặt template làm mặc định
        /// </summary>
        [HttpPost("SetAsDefault/{id}")]
        public async Task<IActionResult> SetAsDefault(int id, [FromForm] string templateType)
        {
            try
            {
                var response = await _apiService.PostAsync<bool>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.PrintTemplates.SetDefault, id), 
                    new { TemplateType = templateType });
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Đặt template làm mặc định thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Lỗi khi đặt template làm mặc định" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi đặt template làm mặc định: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa template
        /// </summary>
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync<bool>(ApiEndpoints.Builder.WithId(ApiEndpoints.PrintTemplates.Delete, id));
                
                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa template thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Lỗi khi xóa template" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa template: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tạo mẫu in mới
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreatePrintTemplateDto templateDto)
        {
            try
            {
                var response = await _apiService.PostAsync<PrintTemplateDto>(
                    ApiEndpoints.PrintTemplates.Create,
                    templateDto
                );

                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Tạo mẫu in thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Lỗi khi tạo mẫu in" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tạo mẫu in: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật mẫu in
        /// </summary>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePrintTemplateDto templateDto)
        {
            try
            {
                var response = await _apiService.PutAsync<PrintTemplateDto>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.PrintTemplates.Update, id),
                    templateDto
                );

                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Cập nhật mẫu in thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Lỗi khi cập nhật mẫu in" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi cập nhật mẫu in: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xem chi tiết mẫu in
        /// </summary>
        [HttpGet("View/{id}")]
        public async Task<IActionResult> View(int id)
        {
            try
            {
                var response = await _apiService.GetAsync<PrintTemplateDto>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.PrintTemplates.GetById, id)
                );

                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Lấy thông tin mẫu in thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.ErrorMessage ?? "Không tìm thấy mẫu in" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lấy thông tin mẫu in: {ex.Message}" });
            }
        }
    }
}
