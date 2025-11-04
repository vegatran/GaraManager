using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý Phát sinh (Additional Issues) - Proxy cho API calls
    /// </summary>
    [Authorize]
    [Route("AdditionalIssues")]
    public class AdditionalIssuesController : Controller
    {
        private readonly ApiService _apiService;

        public AdditionalIssuesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Lấy danh sách phát sinh theo ServiceOrderId
        /// </summary>
        [HttpGet("GetByServiceOrder/{serviceOrderId}")]
        public async Task<IActionResult> GetByServiceOrderId(int serviceOrderId)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.AdditionalIssues.GetByServiceOrder, serviceOrderId);
                var response = await _apiService.GetAsync<ApiResponse<List<AdditionalIssueDto>>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data.Data ?? new List<AdditionalIssueDto>() });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết một phát sinh
        /// </summary>
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.AdditionalIssues.GetById, id);
                var response = await _apiService.GetAsync<ApiResponse<AdditionalIssueDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data.Data });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo phát sinh mới (với upload ảnh) - Sử dụng multipart/form-data
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] CreateAdditionalIssueDto createDto, [FromForm] List<IFormFile>? photos = null)
        {
            try
            {
                var endpoint = ApiEndpoints.AdditionalIssues.Create;
                var response = await _apiService.PostWithFilesAsync<AdditionalIssueDto>(endpoint, createDto, photos);
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Đã tạo phát sinh thành công" });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật phát sinh (với upload ảnh mới, xóa ảnh cũ) - Sử dụng multipart/form-data
        /// </summary>
        [HttpPut("Update/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateAdditionalIssueDto updateDto, [FromForm] List<IFormFile>? newPhotos = null)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.AdditionalIssues.Update, id);
                var response = await _apiService.PutWithFilesAsync<AdditionalIssueDto>(endpoint, updateDto, newPhotos);
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Đã cập nhật phát sinh thành công" });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa phát sinh
        /// </summary>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.AdditionalIssues.Delete, id);
                var response = await _apiService.DeleteAsync<ApiResponse<bool>>(endpoint);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = response.Data?.Message ?? "Đã xóa phát sinh thành công" });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }

        /// <summary>
        /// Upload thêm ảnh cho phát sinh
        /// </summary>
        [HttpPost("UploadPhotos/{id}")]
        public async Task<IActionResult> UploadPhotos(int id, [FromForm] List<IFormFile> photos)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.AdditionalIssues.UploadPhotos, id);
                var response = await _apiService.PostWithFilesAsync<List<AdditionalIssuePhotoDto>>(endpoint, null, photos);
                
                if (response.Success)
                {
                    return Json(new { success = true, data = response.Data, message = "Đã tải lên ảnh thành công" });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa ảnh của phát sinh
        /// </summary>
        [HttpDelete("DeletePhoto/{id}/photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int id, int photoId)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithParameters(ApiEndpoints.AdditionalIssues.DeletePhoto, id, photoId);
                var response = await _apiService.DeleteAsync<ApiResponse<bool>>(endpoint);
                
                if (response.Success)
                {
                    return Json(new { success = true, message = response.Data?.Message ?? "Đã xóa ảnh thành công" });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 2.3.3: Tạo báo giá từ phát sinh
        /// </summary>
        [HttpPost("CreateQuotation/{id}")]
        public async Task<IActionResult> CreateQuotation(int id, [FromBody] CreateQuotationFromIssueDto createDto)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.AdditionalIssues.CreateQuotation, id);
                var response = await _apiService.PostAsync<ApiResponse<ServiceQuotationDto>>(endpoint, createDto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data.Data, message = "Đã tạo báo giá bổ sung từ phát sinh thành công" });
                }
                
                return Json(new { success = false, message = response.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi không xác định: " + ex.Message });
            }
        }
    }
}

