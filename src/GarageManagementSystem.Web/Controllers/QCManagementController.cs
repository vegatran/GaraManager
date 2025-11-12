using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// ✅ 2.4: Controller quản lý Quality Control (QC) và Bàn giao
    /// </summary>
    [Authorize]
    [Route("QCManagement")]
    public class QCManagementController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<QCManagementController>? _logger;

        public QCManagementController(ApiService apiService, ILogger<QCManagementController>? logger = null)
        {
            _apiService = apiService;
            _logger = logger;
        }

        /// <summary>
        /// ✅ 2.4.2: Hiển thị trang Quản Lý QC
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ✅ 2.4.1: Hoàn thành Kỹ thuật
        /// </summary>
        [HttpPost("CompleteTechnical/{id}")]
        public async Task<IActionResult> CompleteTechnical(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.QualityControl.CompleteTechnical, id);
                var response = await _apiService.PostAsync<ApiResponse<ServiceOrderDto>>(endpoint, new { });
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<ServiceOrderDto>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ 2.4.1: Lấy tổng giờ công thực tế
        /// </summary>
        [HttpGet("GetTotalActualHours/{id}")]
        public async Task<IActionResult> GetTotalActualHours(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.QualityControl.GetTotalActualHours, id);
                var response = await _apiService.GetAsync<ApiResponse<decimal>>(endpoint);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<decimal>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Lấy danh sách JO chờ QC với pagination
        /// </summary>
        [HttpGet("GetWaitingForQC")]
        public async Task<IActionResult> GetWaitingForQC(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var endpoint = $"{ApiEndpoints.QualityControl.GetWaitingForQC}?pageNumber={pageNumber}&pageSize={pageSize}";
                var response = await _apiService.GetAsync<PagedResponse<ServiceOrderDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    var orderList = response.Data.Data.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        customerName = o.Customer?.Name ?? "Không xác định",
                        vehiclePlate = o.Vehicle?.LicensePlate ?? "Không xác định",
                        completedDate = o.CompletedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa hoàn thành",
                        totalActualHours = o.TotalActualHours?.ToString("F2") ?? "0.00",
                        qcFailedCount = o.QCFailedCount
                    }).Cast<object>().ToList();

                    return Json(new
                    {
                        success = true,
                        data = orderList,
                        totalCount = response.Data.TotalCount,
                        pageNumber = response.Data.PageNumber,
                        pageSize = response.Data.PageSize
                    });
                }

                return Json(new
                {
                    success = false,
                    data = new List<object>(),
                    totalCount = 0,
                    pageNumber,
                    pageSize,
                    message = response.ErrorMessage ?? "Lỗi khi lấy danh sách JO chờ QC"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    data = new List<object>(),
                    totalCount = 0,
                    pageNumber,
                    pageSize,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Lấy thông tin QC của JO
        /// </summary>
        [HttpGet("GetQC/{id}")]
        public async Task<IActionResult> GetQC(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.QualityControl.GetQC, id);
                var response = await _apiService.GetAsync<ApiResponse<QualityControlDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data.Data });
                }

                return Json(new { success = false, message = response.ErrorMessage ?? "Không tìm thấy thông tin QC" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Bắt đầu kiểm tra QC
        /// </summary>
        [HttpPost("StartQC/{id}")]
        public async Task<IActionResult> StartQC(int id, [FromBody] CreateQualityControlDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.QualityControl.StartQC, id);
                var response = await _apiService.PostAsync<ApiResponse<QualityControlDto>>(endpoint, dto);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<QualityControlDto>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ 2.4.2: Hoàn thành QC với kết quả (Pass/Fail)
        /// </summary>
        [HttpPost("CompleteQC/{id}")]
        public async Task<IActionResult> CompleteQC(int id, [FromBody] CompleteQCDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.QualityControl.CompleteQC, id);
                var response = await _apiService.PostAsync<ApiResponse<QualityControlDto>>(endpoint, dto);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<QualityControlDto>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ 2.4.3: Ghi nhận QC không đạt
        /// </summary>
        [HttpPost("FailQC/{id}")]
        public async Task<IActionResult> FailQC(int id, [FromBody] CompleteQCDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.QualityControl.FailQC, id);
                var response = await _apiService.PostAsync<ApiResponse<ServiceOrderDto>>(endpoint, dto);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<ServiceOrderDto>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ 2.4.4: Bàn giao xe
        /// </summary>
        [HttpPost("Handover/{id}")]
        public async Task<IActionResult> Handover(int id, [FromBody] HandoverServiceOrderDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.QualityControl.Handover, id);
                var response = await _apiService.PostAsync<ApiResponse<ServiceOrderDto>>(endpoint, dto);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<ServiceOrderDto>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ 2.4.3: Ghi nhận giờ công làm lại cho ServiceOrderItem
        /// </summary>
        [HttpPost("RecordReworkHours/{id}/{itemId}")]
        public async Task<IActionResult> RecordReworkHours(int id, int itemId, [FromBody] RecordReworkHoursDto dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.QualityControl.RecordReworkHours, id, itemId);
                var response = await _apiService.PostAsync<ApiResponse<ServiceOrderDto>>(endpoint, dto);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<ServiceOrderDto>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Lấy QC Template để tạo QC Checklist
        /// </summary>
        [HttpGet("GetQCTemplate")]
        public async Task<IActionResult> GetQCTemplate([FromQuery] string? vehicleType = null, [FromQuery] string? serviceType = null)
        {
            try
            {
                // ✅ FIX: Sanitize input để tránh injection và lỗi
                var safeVehicleType = string.IsNullOrWhiteSpace(vehicleType) ? null : vehicleType.Trim();
                var safeServiceType = string.IsNullOrWhiteSpace(serviceType) ? null : serviceType.Trim();
                
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(safeVehicleType))
                    queryParams.Add($"vehicleType={Uri.EscapeDataString(safeVehicleType)}");
                if (!string.IsNullOrEmpty(safeServiceType))
                    queryParams.Add($"serviceType={Uri.EscapeDataString(safeServiceType)}");

                var endpoint = $"{ApiEndpoints.QualityControl.GetQCTemplate}";
                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);

                // ✅ FIX: API trả về ApiResponse<QCChecklistTemplateDto>
                // ApiService.GetAsync<ApiResponse<QCChecklistTemplateDto>> sẽ deserialize thành ApiResponse<QCChecklistTemplateDto>
                // Sau đó wrap lại thành ApiResponse<ApiResponse<QCChecklistTemplateDto>>
                // Vậy response.Data là ApiResponse<QCChecklistTemplateDto>, và response.Data.Data là QCChecklistTemplateDto
                var response = await _apiService.GetAsync<ApiResponse<QCChecklistTemplateDto>>(endpoint);
                
                // ✅ FIX: Đảm bảo response có data và TemplateItems không null
                if (response != null && response.Success && response.Data != null && response.Data.Data != null)
                {
                    // ✅ FIX: response.Data là ApiResponse<QCChecklistTemplateDto>, response.Data.Data là QCChecklistTemplateDto
                    var templateDto = response.Data.Data;
                    if (templateDto.TemplateItems == null)
                    {
                        templateDto.TemplateItems = new List<QCChecklistTemplateItemDto>();
                    }
                }
                
                // ✅ FIX: Return response.Data (là ApiResponse<QCChecklistTemplateDto>) hoặc error response
                if (response == null || !response.Success || response.Data == null)
                {
                    return Json(new ApiResponse<QCChecklistTemplateDto>
                    {
                        Success = false,
                        Message = response?.ErrorMessage ?? "Không nhận được phản hồi từ API"
                    });
                }
                
                // ✅ FIX: Kiểm tra response.Data.Success và response.Data.Data trước khi return
                // response.Data là ApiResponse<QCChecklistTemplateDto>
                if (!response.Data.Success || response.Data.Data == null)
                {
                    return Json(new ApiResponse<QCChecklistTemplateDto>
                    {
                        Success = false,
                        Message = response.Data.ErrorMessage ?? response.Data.Message ?? "Không tìm thấy template"
                    });
                }
                
                return Json(response.Data);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting QC template. VehicleType: {VehicleType}, ServiceType: {ServiceType}", vehicleType, serviceType);
                return Json(new ApiResponse<QCChecklistTemplateDto>
                {
                    Success = false,
                    Message = $"Lỗi khi lấy template: {ex.Message}"
                });
            }
        }
    }
}

