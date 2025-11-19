using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using System;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý Mua hàng (Procurement) - Phase 4.2
    /// </summary>
    [Authorize]
    [Route("Procurement")]
    public class ProcurementController : Controller
    {
        private readonly ApiService _apiService;

        public ProcurementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang Phân tích Nhu cầu (Demand Analysis)
        /// </summary>
        [HttpGet("DemandAnalysis")]
        public IActionResult DemandAnalysis()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách nhu cầu cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetDemandAnalysis")]
        public async Task<IActionResult> GetDemandAnalysis(
            int pageNumber = 1,
            int pageSize = 20,
            int? warehouseId = null,
            string? priority = null,
            string? source = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (warehouseId.HasValue)
                    queryParams.Add($"warehouseId={warehouseId.Value}");
                if (!string.IsNullOrEmpty(priority))
                    queryParams.Add($"priority={Uri.EscapeDataString(priority)}");
                if (!string.IsNullOrEmpty(source))
                    queryParams.Add($"source={Uri.EscapeDataString(source)}");

                var endpoint = ApiEndpoints.Procurement.DemandAnalysis + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<DemandAnalysisDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<DemandAnalysisDto>
                    {
                        Data = new List<DemandAnalysisDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách nhu cầu"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<DemandAnalysisDto>
                {
                    Data = new List<DemandAnalysisDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách đề xuất đặt hàng cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetReorderSuggestions")]
        public async Task<IActionResult> GetReorderSuggestions(
            int pageNumber = 1,
            int pageSize = 50,
            bool includeServiceOrders = true,
            string? minPriority = null,
            bool? isProcessed = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}",
                    $"includeServiceOrders={includeServiceOrders}"
                };

                if (!string.IsNullOrEmpty(minPriority))
                    queryParams.Add($"minPriority={Uri.EscapeDataString(minPriority)}");
                if (isProcessed.HasValue)
                    queryParams.Add($"isProcessed={isProcessed.Value}");

                var endpoint = ApiEndpoints.Procurement.ReorderSuggestions + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<ReorderSuggestionDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<ReorderSuggestionDto>
                    {
                        Data = new List<ReorderSuggestionDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách đề xuất"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<ReorderSuggestionDto>
                {
                    Data = new List<ReorderSuggestionDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo PO từ danh sách suggestions
        /// </summary>
        [HttpPost("BulkCreatePO")]
        public async Task<IActionResult> BulkCreatePO([FromBody] BulkCreatePODto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.Procurement.BulkCreatePO;
                var response = await _apiService.PostAsync<PurchaseOrderDto>(endpoint, dto);
                
                // ✅ FIX: Return response directly (ApiService already wraps in ApiResponse)
                // JavaScript expects: { success: true, data: { ... }, message: "..." }
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<PurchaseOrderDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách suppliers cho dropdown
        /// </summary>
        [HttpGet("GetSuppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            try
            {
                // ✅ FIX: Suppliers API trả về PagedResponse, cần query với pageSize lớn để lấy tất cả
                var endpoint = ApiEndpoints.Suppliers.GetAll + "?pageNumber=1&pageSize=1000";
                var response = await _apiService.GetAsync<PagedResponse<SupplierDto>>(endpoint);
                
                if (response.Success && response.Data != null && response.Data.Data != null)
                {
                    // Filter only active suppliers
                    var activeSuppliers = response.Data.Data
                        .Where(s => s.IsActive)
                        .ToList();
                    return Json(activeSuppliers);
                }
                else
                {
                    return Json(new List<SupplierDto>());
                }
            }
            catch (Exception ex)
            {
                return Json(new List<SupplierDto>());
            }
        }

        #region Phase 4.2.4: Performance Evaluation

        /// <summary>
        /// Hiển thị trang Báo Cáo Hiệu Suất Nhà Cung Cấp
        /// </summary>
        [HttpGet("PerformanceReport")]
        public IActionResult PerformanceReport()
        {
            return View();
        }

        /// <summary>
        /// Lấy báo cáo hiệu suất nhà cung cấp
        /// </summary>
        [HttpGet("GetSupplierPerformanceReport")]
        public async Task<IActionResult> GetSupplierPerformanceReport(
            int pageNumber = 1,
            int pageSize = 20,
            int? supplierId = null,
            int? partId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (supplierId.HasValue)
                    queryParams.Add($"supplierId={supplierId.Value}");
                if (partId.HasValue)
                    queryParams.Add($"partId={partId.Value}");
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                var endpoint = ApiEndpoints.Procurement.SupplierPerformanceReport + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<SupplierPerformanceReportDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<SupplierPerformanceReportDto>
                    {
                        Data = new List<SupplierPerformanceReportDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy báo cáo hiệu suất"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<SupplierPerformanceReportDto>
                {
                    Data = new List<SupplierPerformanceReportDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy ranking nhà cung cấp
        /// </summary>
        [HttpGet("GetSupplierRanking")]
        public async Task<IActionResult> GetSupplierRanking(
            string? sortBy = "OverallScore",
            int? topN = null,
            bool worstPerformers = false)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"sortBy={Uri.EscapeDataString(sortBy ?? "OverallScore")}",
                    $"worstPerformers={worstPerformers}"
                };

                if (topN.HasValue)
                    queryParams.Add($"topN={topN.Value}");

                var endpoint = ApiEndpoints.Procurement.SupplierRanking + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<ApiResponse<List<SupplierRankingDto>>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<List<SupplierRankingDto>>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi lấy ranking"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<SupplierRankingDto>>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách cảnh báo hiệu suất
        /// </summary>
        [HttpGet("GetPerformanceAlerts")]
        public async Task<IActionResult> GetPerformanceAlerts(string? severity = null)
        {
            try
            {
                var endpoint = ApiEndpoints.Procurement.PerformanceAlerts;
                if (!string.IsNullOrEmpty(severity))
                    endpoint += $"?severity={Uri.EscapeDataString(severity)}";

                var response = await _apiService.GetAsync<ApiResponse<List<PerformanceAlertDto>>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<List<PerformanceAlertDto>>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi lấy cảnh báo"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<PerformanceAlertDto>>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tính toán lại hiệu suất nhà cung cấp
        /// </summary>
        [HttpPost("CalculatePerformance")]
        public async Task<IActionResult> CalculatePerformance([FromBody] CalculatePerformanceRequestDto? request)
        {
            try
            {
                var response = await _apiService.PostAsync<ApiResponse<object>>(
                    ApiEndpoints.Procurement.CalculatePerformance,
                    request ?? new CalculatePerformanceRequestDto());

                if (response.Success)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<object>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi tính toán hiệu suất"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        #endregion

        /// <summary>
        /// Hiển thị trang So sánh Nhà cung cấp (Supplier Comparison)
        /// </summary>
        [HttpGet("SupplierComparison")]
        public IActionResult SupplierComparison()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách so sánh nhà cung cấp cho một part
        /// </summary>
        [HttpGet("GetSupplierComparison")]
        public async Task<IActionResult> GetSupplierComparison(
            int partId,
            int quantity = 1)
        {
            try
            {
                // ✅ FIX: API expects quantity as nullable, but we always send a value
                // Build query string properly
                var endpoint = $"{ApiEndpoints.Procurement.SupplierComparison}?partId={partId}&quantity={quantity}";
                // ✅ FIX: API returns ApiResponse<List<SupplierComparisonDto>>, so we need to unwrap
                var response = await _apiService.GetAsync<List<SupplierComparisonDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<List<SupplierComparisonDto>>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi lấy danh sách so sánh"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<SupplierComparisonDto>>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Hiển thị trang Đề xuất Nhà cung cấp (Supplier Recommendation)
        /// </summary>
        [HttpGet("SupplierRecommendation")]
        public IActionResult SupplierRecommendation()
        {
            return View();
        }

        /// <summary>
        /// Lấy đề xuất nhà cung cấp tốt nhất cho một part
        /// </summary>
        [HttpGet("GetSupplierRecommendation")]
        public async Task<IActionResult> GetSupplierRecommendation(
            int partId,
            int quantity = 1)
        {
            try
            {
                var endpoint = $"{ApiEndpoints.Procurement.SupplierRecommendation}?partId={partId}&quantity={quantity}";
                var response = await _apiService.GetAsync<SupplierRecommendationDto>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }
                else
                {
                    return Json(new ApiResponse<SupplierRecommendationDto>
                    {
                        Success = false,
                        ErrorMessage = response.ErrorMessage ?? "Lỗi khi lấy đề xuất nhà cung cấp"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<SupplierRecommendationDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        #region Phase 4.2.2 Optional: Request Quotation

        /// <summary>
        /// Hiển thị trang Quản lý Báo giá Nhà cung cấp (Supplier Quotation Management)
        /// </summary>
        [HttpGet("SupplierQuotationManagement")]
        public IActionResult SupplierQuotationManagement()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách báo giá từ suppliers
        /// </summary>
        [HttpGet("GetQuotations")]
        public async Task<IActionResult> GetQuotations(
            int pageNumber = 1,
            int pageSize = 20,
            int? partId = null,
            int? supplierId = null,
            string? status = null)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (partId.HasValue)
                    queryParams.Add($"partId={partId.Value}");
                if (supplierId.HasValue)
                    queryParams.Add($"supplierId={supplierId.Value}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var endpoint = ApiEndpoints.Procurement.GetQuotations + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<SupplierQuotationDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }
                else
                {
                    return Json(new PagedResponse<SupplierQuotationDto>
                    {
                        Data = new List<SupplierQuotationDto>(),
                        TotalCount = 0,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Success = false,
                        Message = response.ErrorMessage ?? "Lỗi khi lấy danh sách báo giá"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new PagedResponse<SupplierQuotationDto>
                {
                    Data = new List<SupplierQuotationDto>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Gửi yêu cầu báo giá cho nhà cung cấp
        /// </summary>
        [HttpPost("RequestQuotation")]
        public async Task<IActionResult> RequestQuotation([FromBody] RequestQuotationDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.Procurement.RequestQuotation;
                var response = await _apiService.PostAsync<RequestQuotationResponseDto>(endpoint, dto);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<RequestQuotationResponseDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật báo giá (supplier response)
        /// </summary>
        [HttpPut("UpdateQuotation/{id}")]
        public async Task<IActionResult> UpdateQuotation(int id, [FromBody] UpdateQuotationDto dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Procurement.UpdateQuotation, id);
                var response = await _apiService.PutAsync<SupplierQuotationDto>(endpoint, dto);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<SupplierQuotationDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Chấp nhận báo giá
        /// </summary>
        [HttpPut("AcceptQuotation/{id}")]
        public async Task<IActionResult> AcceptQuotation(int id, [FromBody] AcceptRejectQuotationDto? dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Procurement.AcceptQuotation, id);
                var response = await _apiService.PutAsync<SupplierQuotationDto>(endpoint, dto ?? new AcceptRejectQuotationDto());
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<SupplierQuotationDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Từ chối báo giá
        /// </summary>
        [HttpPut("RejectQuotation/{id}")]
        public async Task<IActionResult> RejectQuotation(int id, [FromBody] AcceptRejectQuotationDto? dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Procurement.RejectQuotation, id);
                var response = await _apiService.PutAsync<SupplierQuotationDto>(endpoint, dto ?? new AcceptRejectQuotationDto());
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<SupplierQuotationDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết báo giá theo ID
        /// </summary>
        [HttpGet("GetQuotation/{id}")]
        public async Task<IActionResult> GetQuotation(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.Procurement.GetQuotationById, id);
                var response = await _apiService.GetAsync<SupplierQuotationDto>(endpoint);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<SupplierQuotationDto>
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách parts cho dropdown
        /// </summary>
        [HttpGet("GetParts")]
        public async Task<IActionResult> GetParts()
        {
            try
            {
                var endpoint = ApiEndpoints.Parts.GetAll + "?pageNumber=1&pageSize=1000";
                var response = await _apiService.GetAsync<PagedResponse<PartDto>>(endpoint);
                
                if (response.Success && response.Data != null && response.Data.Data != null)
                {
                    // Filter only active parts (API should already filter deleted, but double check)
                    var activeParts = response.Data.Data
                        .Where(p => p.IsActive)
                        .ToList();
                    return Json(activeParts);
                }
                else
                {
                    return Json(new List<PartDto>());
                }
            }
            catch (Exception ex)
            {
                return Json(new List<PartDto>());
            }
        }

        #endregion
    }

}

