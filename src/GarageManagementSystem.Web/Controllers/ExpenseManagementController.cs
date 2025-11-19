using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// ✅ 4.3.1.7: Controller quản lý Chi Phí (Expense Management)
    /// </summary>
    [Authorize]
    [Route("ExpenseManagement")]
    public class ExpenseManagementController : Controller
    {
        private readonly ApiService _apiService;

        public ExpenseManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// ✅ 4.3.1.7: Hiển thị trang quản lý Chi Phí
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ✅ 4.3.1.7: Lấy danh sách Expenses với pagination và filters
        /// </summary>
        [HttpGet("GetExpenses")]
        public async Task<IActionResult> GetExpenses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null,
            [FromQuery] bool? isApproved = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}",
                    "transactionType=Expense" // Chỉ lấy Expense
                };

                if (!string.IsNullOrEmpty(category))
                {
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");
                }

                if (!string.IsNullOrEmpty(status))
                {
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                }

                if (fromDate.HasValue)
                {
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }

                var endpoint = ApiEndpoints.FinancialTransactions.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<FinancialTransactionDto>>(endpoint);

                if (response != null && response.Success && response.Data != null)
                {
                    // Filter by isApproved if specified
                    if (isApproved.HasValue)
                    {
                        response.Data.Data = response.Data.Data.Where(t => t.IsApproved == isApproved.Value).ToList();
                        response.Data.TotalCount = response.Data.Data.Count();
                    }

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
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy danh sách chi phí" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy danh sách chi phí: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.7: Lấy thống kê chi phí
        /// </summary>
        [HttpGet("GetExpenseSummary")]
        public async Task<IActionResult> GetExpenseSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var queryParams = new List<string> { "transactionType=Expense" };

                if (fromDate.HasValue)
                {
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }

                var endpoint = ApiEndpoints.FinancialTransactions.GetAll + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<PagedResponse<FinancialTransactionDto>>(endpoint);

                if (response != null && response.Success && response.Data != null)
                {
                    var expenses = response.Data.Data;
                    var totalExpense = expenses.Sum(e => e.Amount);
                    var pendingExpense = expenses.Where(e => e.Status == "Pending").Sum(e => e.Amount);
                    var approvedExpense = expenses.Where(e => e.IsApproved).Sum(e => e.Amount);
                    var categoryBreakdown = expenses
                        .GroupBy(e => e.Category)
                        .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
                        .OrderByDescending(x => x.Total)
                        .ToList();

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            totalExpense,
                            pendingExpense,
                            approvedExpense,
                            categoryBreakdown,
                            totalCount = expenses.Count(),
                            pendingCount = expenses.Count(e => e.Status == "Pending"),
                            approvedCount = expenses.Count(e => e.IsApproved)
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy thống kê chi phí" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy thống kê chi phí: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.7: Lấy danh sách categories cho filter
        /// </summary>
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var response = await _apiService.GetAsync<ApiResponse<List<string>>>(ApiEndpoints.FinancialTransactions.GetCategories);

                if (response != null && response.Success && response.Data != null && response.Data.Data != null && response.Data.Data.Any())
                {
                    // Filter categories that are typically expenses
                    var expenseCategories = response.Data.Data
                        .Where(c => !string.IsNullOrEmpty(c) && 
                            (c.Contains("Purchase") || c.Contains("Cost") || c.Contains("Salary") || 
                             c.Contains("Expense") || c.Contains("Rent") || c.Contains("Utility")))
                        .ToList();

                    return Json(expenseCategories);
                }

                return Json(new List<string>());
            }
            catch (Exception ex)
            {
                return Json(new List<string>());
            }
        }

        /// <summary>
        /// ✅ 4.3.1.7: Tạo Expense mới
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateFinancialTransactionDto dto)
        {
            try
            {
                // Ensure transaction type is Expense
                dto.TransactionType = "Expense";

                var endpoint = ApiEndpoints.FinancialTransactions.Create;
                var response = await _apiService.PostAsync<FinancialTransactionDto>(endpoint, dto);

                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Tạo phiếu chi thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi tạo phiếu chi" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi tạo phiếu chi: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.7: Cập nhật Expense
        /// </summary>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateFinancialTransactionDto dto)
        {
            try
            {
                // Ensure transaction type is Expense
                dto.TransactionType = "Expense";

                var endpoint = string.Format(ApiEndpoints.FinancialTransactions.Update, id);
                var response = await _apiService.PutAsync<FinancialTransactionDto>(endpoint, dto);

                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Cập nhật phiếu chi thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi cập nhật phiếu chi" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi cập nhật phiếu chi: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.7: Xóa Expense
        /// </summary>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.FinancialTransactions.Delete, id);
                var response = await _apiService.DeleteAsync<ApiResponse<bool>>(endpoint);

                if (response.Success)
                {
                    return Json(new { success = true, message = "Xóa phiếu chi thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi xóa phiếu chi" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi xóa phiếu chi: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.7: Lấy Expense theo ID
        /// </summary>
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.FinancialTransactions.GetById, id);
                var response = await _apiService.GetAsync<ApiResponse<FinancialTransactionDto>>(endpoint);

                if (response != null && response.Success && response.Data != null && response.Data.Data != null)
                {
                    // Verify it's an Expense
                    if (response.Data.Data.TransactionType != "Expense")
                    {
                        return Json(new { success = false, error = "Không phải phiếu chi" });
                    }

                    return Json(new { success = true, data = response.Data.Data });
                }
                else
                {
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Không tìm thấy phiếu chi" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy thông tin phiếu chi: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.7: Lấy danh sách Employees cho dropdown
        /// </summary>
        [HttpGet("GetAvailableEmployees")]
        public async Task<IActionResult> GetAvailableEmployees()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<EmployeeDto>>(ApiEndpoints.Employees.GetAll + "?pageNumber=1&pageSize=1000");

                if (response != null && response.Success && response.Data != null && response.Data.Data != null && response.Data.Data.Any())
                {
                    var employees = response.Data.Data.Select(e => new
                    {
                        value = e.Id.ToString(),
                        text = e.Name
                    }).Cast<object>().ToList();

                    return Json(employees);
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

