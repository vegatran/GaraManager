using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý đơn hàng sửa chữa với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("OrderManagement")]
    public class OrderManagementController : Controller
    {
        private readonly ApiService _apiService;

        public OrderManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý đơn hàng sửa chữa
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách tất cả đơn hàng sửa chữa cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetOrders")]
        public async Task<IActionResult> GetOrders()
        {
            var response = await _apiService.GetAsync<List<ServiceOrderDto>>(ApiEndpoints.ServiceOrders.GetAll);
            
            if (response.Success)
            {
                var orderList = new List<object>();
                
                if (response.Data != null)
                {
                    orderList = response.Data.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        customerName = o.Customer?.Name ?? "Không xác định",
                        vehiclePlate = o.Vehicle?.LicensePlate ?? "Không xác định",
                        orderDate = o.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                        scheduledDate = o.ScheduledDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa lên lịch",
                        completedDate = o.CompletedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa hoàn thành",
                        status = TranslateOrderStatus(o.Status),
                        statusOriginal = o.Status, // ✅ THÊM: Giữ nguyên status gốc để JS xử lý
                        finalAmount = o.FinalAmount.ToString("N0"),
                        paymentStatus = TranslatePaymentStatus(o.PaymentStatus ?? "Pending"),
                        serviceCount = o.ServiceOrderItems.Count
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = orderList,
                    message = "Lấy danh sách đơn hàng sửa chữa thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// ✅ THÊM: Lấy danh sách đơn hàng với server-side pagination cho DataTable
        /// </summary>
        [HttpGet("GetOrdersPaged")]
        public async Task<IActionResult> GetOrdersPaged(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null)
        {
            try
            {
                // Build query string
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var endpoint = ApiEndpoints.ServiceOrders.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<ServiceOrderDto>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    var orderList = response.Data.Data.Select(o => new
                    {
                        id = o.Id,
                        orderNumber = o.OrderNumber,
                        customerName = o.Customer?.Name ?? "Không xác định",
                        vehiclePlate = o.Vehicle?.LicensePlate ?? "Không xác định",
                        orderDate = o.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                        scheduledDate = o.ScheduledDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa lên lịch",
                        completedDate = o.CompletedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Chưa hoàn thành",
                        status = TranslateOrderStatus(o.Status),
                        statusOriginal = o.Status, // ✅ THÊM: Giữ nguyên status gốc để JS xử lý
                        finalAmount = o.FinalAmount.ToString("N0"),
                        paymentStatus = TranslatePaymentStatus(o.PaymentStatus ?? "Pending"),
                        serviceCount = o.ServiceOrderItems?.Count ?? 0,
                        serviceOrderId = o.Id // ✅ THÊM: Để check trong JS
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
                else
                {
                    return Json(new
                    {
                        success = false,
                        data = new List<object>(),
                        totalCount = 0,
                        pageNumber = pageNumber,
                        pageSize = pageSize,
                        message = response.ErrorMessage ?? "Lỗi khi lấy danh sách đơn hàng"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    data = new List<object>(),
                    totalCount = 0,
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng sửa chữa theo ID thông qua API
        /// </summary>
        [HttpGet("GetOrder/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                // ✅ FIX: API trả về ApiResponse<ServiceOrderDto>, ApiService cũng wrap thành ApiResponse
                // Đổi sang GetAsync<ServiceOrderDto> để tránh double nesting
                var response = await _apiService.GetAsync<ServiceOrderDto>(
                    ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetById, id)
                );
                
                if (response.Success && response.Data != null)
                {
                    // ✅ FIX: response.Data giờ là ServiceOrderDto trực tiếp (không cần .Data.Data)
                    return Json(new { success = true, data = response.Data });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không tìm thấy phiếu sửa chữa" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 2.3.4: Lấy tiến độ Service Order
        /// </summary>
        [HttpGet("GetOrderProgress/{id}")]
        public async Task<IActionResult> GetOrderProgress(int id)
        {
            try
            {
                // ✅ FIX: API trả về ApiResponse<ServiceOrderProgressDto>, ApiService cũng wrap thành ApiResponse
                // Đổi sang GetAsync<ServiceOrderProgressDto> để tránh double nesting
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetProgress, id);
                var response = await _apiService.GetAsync<ServiceOrderProgressDto>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    // ✅ FIX: response.Data giờ là ServiceOrderProgressDto trực tiếp (không cần .Data.Data)
                    return Json(new { success = true, data = response.Data });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không thể lấy tiến độ" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 3.1: Lấy chi tiết COGS cho Service Order
        /// </summary>
        [HttpGet("GetOrderCogs/{id}")]
        public async Task<IActionResult> GetOrderCogs(int id)
        {
            try
            {
                // ✅ FIX: API trả về ApiResponse<COGSBreakdownDto>, ApiService cũng wrap thành ApiResponse
                // Đổi sang GetAsync<COGSBreakdownDto> để tránh double nesting
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetCogsDetails, id);
                var response = await _apiService.GetAsync<COGSBreakdownDto>(endpoint);

                if (response.Success && response.Data != null)
                {
                    // ✅ FIX: response.Data giờ là COGSBreakdownDto trực tiếp (không cần .Data.Data)
                    return Json(new { success = true, data = response.Data, message = response.Message });
                }

                return Json(new { success = false, error = response.ErrorMessage ?? "Không thể lấy chi tiết COGS" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 3.1: Tính lại COGS cho Service Order
        /// </summary>
        [HttpPost("CalculateOrderCogs/{id}")]
        public async Task<IActionResult> CalculateOrderCogs(int id, [FromBody] COGSMethodDto? methodDto)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.CalculateCogs, id);
                var response = await _apiService.PostAsync<COGSCalculationDto>(endpoint, methodDto ?? new COGSMethodDto());
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 3.1: Thiết lập phương pháp tính COGS
        /// </summary>
        [HttpPut("SetOrderCogsMethod/{id}")]
        public async Task<IActionResult> SetOrderCogsMethod(int id, [FromBody] COGSMethodDto methodDto)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.SetCogsMethod, id);
                var response = await _apiService.PutAsync<object>(endpoint, methodDto ?? new COGSMethodDto());
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 3.1: Lấy lợi nhuận gộp cho Service Order
        /// </summary>
        [HttpGet("GetOrderGrossProfit/{id}")]
        public async Task<IActionResult> GetOrderGrossProfit(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetGrossProfit, id);
                // ✅ FIX: Đổi sang GetAsync<GrossProfitDto> để tránh double nesting
                var response = await _apiService.GetAsync<GrossProfitDto>(endpoint);

                if (response.Success && response.Data != null)
                {
                    // ✅ FIX: response.Data giờ là GrossProfitDto trực tiếp (không cần .Data.Data)
                    return Json(new { success = true, data = response.Data, message = response.ErrorMessage ?? "Lấy thông tin lợi nhuận thành công" });
                }

                return Json(new { success = false, error = response.ErrorMessage ?? "Không thể lấy lợi nhuận gộp" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("GetOrderFees/{id}")]
        public async Task<IActionResult> GetOrderFees(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.GetFees, id);
                var response = await _apiService.GetAsync<ApiResponse<ServiceOrderFeeSummaryDto>>(endpoint);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPut("UpdateOrderFees/{id}")]
        public async Task<IActionResult> UpdateOrderFees(int id, [FromBody] UpdateServiceOrderFeesRequestDto request)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.UpdateFees, id);
                var response = await _apiService.PutAsync<ApiResponse<ServiceOrderFeeSummaryDto>>(endpoint, request ?? new UpdateServiceOrderFeesRequestDto());
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("CogsSummary")]
        public IActionResult CogsSummary()
        {
            return View();
        }

        [HttpGet("GetCogsSummary")]
        public async Task<IActionResult> GetCogsSummary(DateTime? startDate, DateTime? endDate, string? method)
        {
            try
            {
                var endpoint = BuildCogsReportEndpoint(ApiEndpoints.ServiceOrders.GetCogsReport, startDate, endDate, method);
                var response = await _apiService.GetAsync<ApiResponse<ServiceOrderCogsReportDto>>(endpoint);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("ExportCogsSummary")]
        public async Task<IActionResult> ExportCogsSummary(DateTime? startDate, DateTime? endDate, string? method)
        {
            try
            {
                var endpoint = BuildCogsReportEndpoint(ApiEndpoints.ServiceOrders.ExportCogsReport, startDate, endDate, method);
                var response = await _apiService.GetByteArrayAsync(endpoint);
                if (!response.Success || response.Data == null)
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không thể xuất báo cáo." });
                }

                var fileName = string.IsNullOrWhiteSpace(response.Message)
                    ? $"BaoCaoCOGS-{DateTime.Now:yyyyMMddHHmmss}.csv"
                    : response.Message.Trim('"');

                return File(response.Data, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet("ExportCogsSummaryExcel")]
        public async Task<IActionResult> ExportCogsSummaryExcel(DateTime? startDate, DateTime? endDate, string? method)
        {
            try
            {
                var endpoint = BuildCogsReportEndpoint(ApiEndpoints.ServiceOrders.ExportCogsReportExcel, startDate, endDate, method);
                var response = await _apiService.GetByteArrayAsync(endpoint);
                if (!response.Success || response.Data == null)
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Không thể xuất báo cáo." });
                }

                var fileName = string.IsNullOrWhiteSpace(response.Message)
                    ? $"BaoCaoCOGS-{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    : response.Message.Trim('"');

                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Lỗi: {ex.Message}" });
            }
        }

        private string BuildCogsReportEndpoint(string baseEndpoint, DateTime? startDate, DateTime? endDate, string? method)
        {
            var queryParams = new List<string>();

            if (startDate.HasValue)
            {
                queryParams.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("o"))}");
            }

            if (endDate.HasValue)
            {
                queryParams.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("o"))}");
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                queryParams.Add($"method={Uri.EscapeDataString(method)}");
            }

            if (queryParams.Count == 0)
            {
                return baseEndpoint;
            }

            return $"{baseEndpoint}?{string.Join("&", queryParams)}";
        }

        /// <summary>
        /// ✅ 3.2: Tạo hoặc lấy bảo hành cho Service Order
        /// </summary>
        [HttpPost("GenerateWarranty/{id}")]
        public async Task<IActionResult> GenerateWarranty(int id, [FromBody] GenerateWarrantyRequestDto? request)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.Warranties.GenerateForServiceOrder, id);
                var response = await _apiService.PostAsync<WarrantyDto>(endpoint, request ?? new GenerateWarrantyRequestDto());
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 3.2: Lấy thông tin bảo hành của Service Order
        /// </summary>
        [HttpGet("GetWarranty/{id}")]
        public async Task<IActionResult> GetWarranty(int id)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.Warranties.GetByServiceOrder, id);
                var response = await _apiService.GetAsync<WarrantyDto>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response);
                }

                var friendlyMessage = ExtractFriendlyWarrantyMessage(response);
                return Json(ApiResponse<WarrantyDto>.ErrorResult(friendlyMessage));
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"Lỗi: {ex.Message}" });
            }
        }

        private string ExtractFriendlyWarrantyMessage(ApiResponse<WarrantyDto> response)
        {
            if (response == null)
            {
                return "Không tìm thấy bảo hành cho phiếu sửa chữa này";
            }

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                var jsonMatch = Regex.Match(response.ErrorMessage, @"\{.*\}");
                if (jsonMatch.Success)
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var inner = JsonSerializer.Deserialize<ApiResponse<WarrantyDto>>(jsonMatch.Value, options);
                        if (inner != null)
                        {
                            if (!string.IsNullOrWhiteSpace(inner.Message))
                            {
                                return inner.Message;
                            }

                            if (!string.IsNullOrWhiteSpace(inner.ErrorMessage))
                            {
                                return inner.ErrorMessage;
                            }
                        }
                    }
                    catch
                    {
                        // ignore parse errors
                    }
                }

                return response.ErrorMessage;
            }

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                return response.Message;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return "Không tìm thấy bảo hành cho phiếu sửa chữa này";
            }

            return "Không thể lấy thông tin bảo hành";
        }

        /// <summary>
        /// ✅ 3.2: Tạo khiếu nại bảo hành cho một Warranty
        /// </summary>
        [HttpPost("CreateWarrantyClaim/{warrantyId}")]
        public async Task<IActionResult> CreateWarrantyClaim(int warrantyId, [FromBody] CreateWarrantyClaimDto request)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.Warranties.CreateClaim, warrantyId);
                var response = await _apiService.PostAsync<WarrantyClaimDto>(endpoint, request ?? new CreateWarrantyClaimDto { IssueDescription = string.Empty });
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// ✅ 3.2: Cập nhật trạng thái khiếu nại bảo hành
        /// </summary>
        [HttpPut("UpdateWarrantyClaim/{claimId}")]
        public async Task<IActionResult> UpdateWarrantyClaim(int claimId, [FromBody] UpdateWarrantyClaimStatusDto request)
        {
            try
            {
                var endpoint = ApiEndpoints.Builder.WithId(ApiEndpoints.Warranties.UpdateClaim, claimId);
                var response = await _apiService.PutAsync<WarrantyClaimDto>(endpoint, request ?? new UpdateWarrantyClaimStatusDto());
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"Lỗi: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách ServiceQuotation đã được phê duyệt để tạo phiếu sửa chữa
        /// </summary>
        [HttpGet("GetAvailableQuotations")]
        public async Task<IActionResult> GetAvailableQuotations()
        {
            try
            {
                // ✅ Dùng API chuyên biệt để lấy danh sách báo giá đã duyệt, đủ điều kiện tạo SO
                // ✅ FIX: Đổi sang GetAsync<List<object>> để tránh double nesting
                var response = await _apiService.GetAsync<List<object>>(ApiEndpoints.ServiceQuotations.GetApprovedAvailableForOrder);
                
                if (response.Success && response.Data != null)
                {
                    // ✅ FIX: response.Data giờ là List<object> trực tiếp (không cần .Data.Data)
                    return Json(response.Data);
                }
                else
                {
                    // ✅ THÊM: Log lỗi nếu có
                    Console.WriteLine($"[GetAvailableQuotations] API Error: {response.ErrorMessage ?? "Unknown error"}");
                    return Json(new List<object>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAvailableQuotations] Exception: {ex.Message}");
                Console.WriteLine($"[GetAvailableQuotations] StackTrace: {ex.StackTrace}");
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// Lấy danh sách khách hàng cho dropdown (legacy - không dùng trong quy trình mới)
        /// </summary>
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success)
            {
                var customers = response.Data?.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    phone = c.Phone ?? ""
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = customers });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy danh sách xe theo khách hàng
        /// </summary>
        [HttpGet("GetVehiclesByCustomer/{customerId}")]
        public async Task<IActionResult> GetVehiclesByCustomer(int customerId)
        {
            var response = await _apiService.GetAsync<List<VehicleDto>>(ApiEndpoints.Builder.WithId(ApiEndpoints.Vehicles.GetByCustomerId, customerId));
            
            if (response.Success)
            {
                var vehicles = response.Data?.Select(v => new
                {
                    id = v.Id,
                    licensePlate = v.LicensePlate,
                    brand = v.Brand,
                    model = v.Model,
                    displayText = $"{v.LicensePlate} - {v.Brand} {v.Model}"
                }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = vehicles });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Lấy danh sách dịch vụ có sẵn
        /// </summary>
        [HttpGet("GetServices")]
        public async Task<IActionResult> GetServices()
        {
            var response = await _apiService.GetAsync<List<ServiceDto>>(ApiEndpoints.Services.GetAll);
            
            if (response.Success)
            {
                var services = response.Data?
                    .Where(s => s.IsActive)
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        price = s.Price,
                        duration = s.Duration,
                        category = s.Category ?? "General"
                    }).Cast<object>().ToList() ?? new List<object>();

                return Json(new { success = true, data = services });
            }
            else
            {
                return Json(new { success = false, error = response.ErrorMessage });
            }
        }

        /// <summary>
        /// Tạo đơn hàng sửa chữa mới thông qua API
        /// </summary>
        [HttpPost]
        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateServiceOrderDto model)
        {
            // Cho phép tạo từ Báo Giá mà không gửi kèm ServiceOrderItems (API sẽ tự copy từ Quotation)
            if (!ModelState.IsValid)
            {
                if (!model.ServiceQuotationId.HasValue)
                {
                    return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
                }
                // Bỏ qua lỗi validation của ServiceOrderItems khi có ServiceQuotationId
                if (ModelState.ContainsKey("ServiceOrderItems"))
                {
                    ModelState.Remove("ServiceOrderItems");
                }
            }

            var response = await _apiService.PostAsync<ServiceOrderDto>(ApiEndpoints.ServiceOrders.Create, model);
            
            return Json(response);
        }

        /// <summary>
        /// Cập nhật đơn hàng sửa chữa thông qua API
        /// </summary>
        [HttpPut("UpdateOrder/{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateServiceOrderDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            if (id != model.Id)
            {
                return BadRequest(new { success = false, errorMessage = "ID không khớp" });
            }

            var response = await _apiService.PutAsync<ServiceOrderDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.Update, id), model);
            
            return Json(response);
        }

        /// <summary>
        /// Xóa đơn hàng sửa chữa thông qua API
        /// </summary>
        [HttpDelete("DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var response = await _apiService.DeleteAsync<ServiceOrderDto>(ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.Delete, id));
            
            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách nhân viên đang hoạt động cho dropdown
        /// </summary>
        [HttpGet("GetActiveEmployees")]
        public async Task<IActionResult> GetActiveEmployees()
        {
            // ✅ FIX: Đổi sang GetAsync<List<EmployeeDto>> để tránh double nesting
            var response = await _apiService.GetAsync<List<EmployeeDto>>(ApiEndpoints.Employees.GetActive);
            
            if (response.Success && response.Data != null)
            {
                // ✅ FIX: response.Data giờ là List<EmployeeDto> trực tiếp (không cần .Data.Data)
                var employeeList = response.Data.Select(e => new
                {
                    id = e.Id,
                    text = e.Name + " - " + (e.Position ?? "")
                }).Cast<object>().ToList();

                return Json(employeeList);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// ✅ 2.1.1: Chuyển trạng thái ServiceOrder
        /// </summary>
        [HttpPut("ChangeOrderStatus/{id}")]
        public async Task<IActionResult> ChangeOrderStatus(int id, [FromBody] ChangeServiceOrderStatusDto statusDto)
        {
            var response = await _apiService.PutAsync<ServiceOrderDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.ServiceOrders.ChangeStatus, id),
                statusDto
            );
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.1.2: Phân công KTV cho một item
        /// </summary>
        [HttpPut("AssignTechnician/{orderId}/items/{itemId}")]
        public async Task<IActionResult> AssignTechnicianToItem(int orderId, int itemId, [FromBody] AssignTechnicianDto assignDto)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/items/{itemId}/assign-technician";
            var response = await _apiService.PutAsync<ServiceOrderDto>(endpoint, assignDto);
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.1.2: Phân công hàng loạt
        /// </summary>
        [HttpPut("BulkAssignTechnician/{orderId}")]
        public async Task<IActionResult> BulkAssignTechnician(int orderId, [FromBody] BulkAssignTechnicianDto bulkDto)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/bulk-assign-technician";
            var response = await _apiService.PutAsync<ServiceOrderDto>(endpoint, bulkDto);
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.1.2: Cập nhật giờ công dự kiến
        /// </summary>
        [HttpPut("SetEstimatedHours/{orderId}/items/{itemId}")]
        public async Task<IActionResult> SetEstimatedHours(int orderId, int itemId, [FromBody] decimal estimatedHours)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/items/{itemId}/set-estimated-hours";
            var response = await _apiService.PutAsync<ServiceOrderDto>(endpoint, estimatedHours);
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.3.1: KTV bắt đầu làm việc cho một item
        /// </summary>
        [HttpPost("StartItemWork/{orderId}/items/{itemId}")]
        public async Task<IActionResult> StartItemWork(int orderId, int itemId)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/items/{itemId}/start-work";
            var response = await _apiService.PostAsync<ServiceOrderDto>(endpoint, null);
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.3.1: KTV dừng làm việc cho một item
        /// </summary>
        [HttpPost("StopItemWork/{orderId}/items/{itemId}")]
        public async Task<IActionResult> StopItemWork(int orderId, int itemId, [FromBody] decimal? actualHours = null)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/items/{itemId}/stop-work";
            var response = await _apiService.PostAsync<ServiceOrderDto>(endpoint, actualHours);
            return Json(response);
        }

        /// <summary>
        /// ✅ 2.3.1 & 2.3.4: KTV hoàn thành một item
        /// </summary>
        [HttpPost("CompleteItem/{orderId}/items/{itemId}")]
        public async Task<IActionResult> CompleteItem(int orderId, int itemId, [FromBody] decimal? actualHours = null)
        {
            var endpoint = $"/api/ServiceOrders/{orderId}/items/{itemId}/complete";
            var response = await _apiService.PostAsync<ServiceOrderDto>(endpoint, actualHours);
            return Json(response);
        }

        private static string TranslateOrderStatus(string status)
        {
            return status switch
            {
                "Pending" => "Chờ Xử Lý",
                "PendingAssignment" => "Chờ Phân Công",
                "WaitingForParts" => "Đang Chờ Vật Tư",
                "ReadyToWork" => "Sẵn Sàng Làm",
                "Confirmed" => "Đã Xác Nhận",
                "InProgress" => "Đang Thực Hiện",
                "Completed" => "Hoàn Thành",
                "Cancelled" => "Đã Hủy",
                "OnHold" => "Tạm Dừng",
                _ => status
            };
        }

        private static string TranslatePaymentStatus(string status)
        {
            return status switch
            {
                "Pending" => "Chờ Thanh Toán",
                "Paid" => "Đã Thanh Toán",
                "Partial" => "Thanh Toán Một Phần",
                "Overdue" => "Quá Hạn",
                "Refunded" => "Đã Hoàn Tiền",
                _ => status
            };
        }
    }
}

