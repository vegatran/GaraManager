using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// ✅ 4.3.2.2: Controller quản lý Công Nợ Phải Thu (Accounts Receivable Management)
    /// </summary>
    [Authorize]
    [Route("AccountsReceivableManagement")]
    public class AccountsReceivableManagementController : Controller
    {
        private readonly ApiService _apiService;

        public AccountsReceivableManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// ✅ 4.3.2.2: Hiển thị trang quản lý Công Nợ Phải Thu
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ✅ 4.3.2.2: Lấy danh sách công nợ phải thu với pagination và filters
        /// </summary>
        [HttpGet("GetAccountsReceivable")]
        public async Task<IActionResult> GetAccountsReceivable(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? overdueDays = null,
            [FromQuery] string? paymentStatus = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (customerId.HasValue)
                {
                    queryParams.Add($"customerId={customerId.Value}");
                }

                if (fromDate.HasValue)
                {
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }

                if (overdueDays.HasValue)
                {
                    queryParams.Add($"overdueDays={overdueDays.Value}");
                }

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    queryParams.Add($"paymentStatus={Uri.EscapeDataString(paymentStatus)}");
                }

                var endpoint = ApiEndpoints.AccountsReceivable.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<AccountsReceivableDto>>(endpoint);

                // ✅ SỬA: API trả về PagedResponse trực tiếp, ApiService sẽ wrap thành ApiResponse<PagedResponse<T>>
                // Nên cần check response.Data (là PagedResponse) và response.Data.Data (là List<AccountsReceivableDto>)
                if (response != null && response.Success && response.Data != null && response.Data.Data != null)
                {
                    return Json(new
                    {
                        success = true,
                        data = response.Data.Data,
                        totalCount = response.Data.TotalCount,
                        pageNumber = response.Data.PageNumber,
                        pageSize = response.Data.PageSize,
                        totalPages = response.Data.TotalPages
                    });
                }
                else
                {
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy danh sách công nợ phải thu" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy danh sách công nợ phải thu: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.2.2: Lấy thống kê công nợ phải thu
        /// </summary>
        [HttpGet("GetSummary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var queryParams = new List<string>();

                if (customerId.HasValue)
                {
                    queryParams.Add($"customerId={customerId.Value}");
                }

                if (fromDate.HasValue)
                {
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }

                var endpoint = ApiEndpoints.AccountsReceivable.GetSummary;
                if (queryParams.Any())
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _apiService.GetAsync<ApiResponse<AccountsReceivableSummaryDto>>(endpoint);

                if (response != null && response.Success && response.Data != null && response.Data.Data != null)
                {
                    return Json(new { success = true, data = response.Data.Data });
                }
                else
                {
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy thống kê công nợ phải thu" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy thống kê công nợ phải thu: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.2.2: Lấy danh sách Customers cho dropdown
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<CustomerDto>>(ApiEndpoints.Customers.GetAll + "?pageNumber=1&pageSize=1000");

                // ✅ SỬA: Safe null checks và check Any() để tránh lỗi nếu list empty
                if (response != null && response.Success && response.Data != null && response.Data.Data != null && response.Data.Data.Any())
                {
                    var customers = response.Data.Data
                        .Where(c => c != null && !string.IsNullOrEmpty(c.Name)) // ✅ SỬA: Filter null và empty names
                        .Select(c => new
                        {
                            value = c.Id.ToString(),
                            text = c.Name ?? "N/A"
                        })
                        .Cast<object>()
                        .ToList();

                    return Json(customers);
                }

                return Json(new List<object>());
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }
    }
}

