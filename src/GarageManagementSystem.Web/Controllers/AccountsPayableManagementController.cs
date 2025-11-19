using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// ✅ 4.3.2.4: Controller quản lý Công Nợ Phải Trả (Accounts Payable Management)
    /// </summary>
    [Authorize]
    [Route("AccountsPayableManagement")]
    public class AccountsPayableManagementController : Controller
    {
        private readonly ApiService _apiService;

        public AccountsPayableManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// ✅ 4.3.2.4: Hiển thị trang quản lý Công Nợ Phải Trả
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ✅ 4.3.2.4: Lấy danh sách công nợ phải trả với pagination và filters
        /// </summary>
        [HttpGet("GetAccountsPayable")]
        public async Task<IActionResult> GetAccountsPayable(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? supplierId = null,
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

                if (supplierId.HasValue)
                {
                    queryParams.Add($"supplierId={supplierId.Value}");
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

                var endpoint = ApiEndpoints.AccountsPayable.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<AccountsPayableDto>>(endpoint);

                // ✅ API trả về PagedResponse trực tiếp, ApiService sẽ wrap thành ApiResponse<PagedResponse<T>>
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
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy danh sách công nợ phải trả" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy danh sách công nợ phải trả: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.2.4: Lấy thống kê công nợ phải trả
        /// </summary>
        [HttpGet("GetSummary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] int? supplierId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var queryParams = new List<string>();

                if (supplierId.HasValue)
                {
                    queryParams.Add($"supplierId={supplierId.Value}");
                }

                if (fromDate.HasValue)
                {
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }

                var endpoint = ApiEndpoints.AccountsPayable.GetSummary;
                if (queryParams.Any())
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await _apiService.GetAsync<ApiResponse<AccountsPayableSummaryDto>>(endpoint);

                if (response != null && response.Success && response.Data != null && response.Data.Data != null)
                {
                    return Json(new { success = true, data = response.Data.Data });
                }
                else
                {
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy thống kê công nợ phải trả" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy thống kê công nợ phải trả: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.2.4: Lấy danh sách Suppliers cho dropdown
        /// </summary>
        [HttpGet("GetAvailableSuppliers")]
        public async Task<IActionResult> GetAvailableSuppliers()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<SupplierDto>>(ApiEndpoints.Suppliers.GetAll + "?pageNumber=1&pageSize=1000");

                // ✅ Safe null checks và check Any() để tránh lỗi nếu list empty
                if (response != null && response.Success && response.Data != null && response.Data.Data != null && response.Data.Data.Any())
                {
                    var suppliers = response.Data.Data
                        .Where(s => s != null && !string.IsNullOrEmpty(s.SupplierName)) // ✅ Filter null và empty names
                        .Select(s => new
                        {
                            value = s.Id.ToString(),
                            text = s.SupplierName ?? "N/A"
                        })
                        .Cast<object>()
                        .ToList();

                    return Json(suppliers);
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

