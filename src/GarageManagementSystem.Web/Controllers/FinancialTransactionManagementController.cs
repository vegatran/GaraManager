using GarageManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;
using GarageManagementSystem.Shared.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý Financial Transactions (Phiếu Thu/Chi)
    /// </summary>
    [Authorize]
    [Route("FinancialTransactionManagement")]
    public class FinancialTransactionManagementController : Controller
    {
        private readonly ApiService _apiService;

        public FinancialTransactionManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý Phiếu Thu/Chi
        /// </summary>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách Financial Transactions với pagination
        /// </summary>
        [HttpGet("GetFinancialTransactions")]
        public async Task<IActionResult> GetFinancialTransactions(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? transactionType = null,
            [FromQuery] string? category = null,
            [FromQuery] string? status = null)
        {
            try
            {
                // Build query parameters
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };
                
                if (!string.IsNullOrEmpty(transactionType))
                    queryParams.Add($"transactionType={Uri.EscapeDataString(transactionType)}");
                if (!string.IsNullOrEmpty(category))
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"{ApiEndpoints.FinancialTransactions.GetAll}?{queryString}";

                // Use PagedResponse pattern like EmployeesController
                var response = await _apiService.GetAsync<PagedResponse<FinancialTransactionDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    var transactionList = response.Data.Data.Select(t => new
                    {
                        id = t.Id,
                        transactionNumber = t.TransactionNumber,
                        transactionType = t.TransactionType,
                        category = t.Category,
                        amount = t.Amount,
                        transactionDate = t.TransactionDate.ToString("yyyy-MM-dd"),
                        employeeName = t.EmployeeName ?? "N/A",
                        status = t.Status,
                        notes = t.Notes ?? t.Description ?? ""
                    }).ToList();

                    return Json(new { 
                        success = true,
                        data = transactionList,
                        totalCount = response.Data.TotalCount,
                        message = "Lấy danh sách phiếu tài chính thành công"
                    });
                }
                else
                {
                    return Json(new { 
                        success = false,
                        error = response.ErrorMessage ?? "Lỗi khi lấy danh sách phiếu tài chính"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    error = "Lỗi khi lấy danh sách phiếu tài chính: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết Financial Transaction theo ID
        /// </summary>
        [HttpGet("GetFinancialTransactionById/{id}")]
        public async Task<IActionResult> GetFinancialTransactionById(int id)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.FinancialTransactions.GetById, id);
                var response = await _apiService.GetAsync<ApiResponse<dynamic>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }

                return Json(new { success = false, message = "Không tìm thấy phiếu tài chính" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách categories
        /// </summary>
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var endpoint = "/api/FinancialTransactions/categories";
                var response = await _apiService.GetAsync<ApiResponse<List<string>>>(endpoint);

                if (response.Success && response.Data != null)
                {
                    return Json(response.Data);
                }

                return Json(new List<string>());
            }
            catch
            {
                return Json(new List<string>());
            }
        }

        /// <summary>
        /// Lấy tổng hợp dữ liệu Financial Transactions
        /// </summary>
        [HttpGet("GetSummary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var response = await _apiService.GetAsync<PagedResponse<FinancialTransactionDto>>(
                    $"{ApiEndpoints.FinancialTransactions.GetAll}?pageSize=1000"); // Get all for summary
                
                if (response.Success && response.Data != null)
                {
                    var transactions = response.Data.Data;
                    
                    var summary = new
                    {
                        totalIncome = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount),
                        totalExpense = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount),
                        pendingCount = transactions.Count(t => t.Status == "Pending"),
                        completedCount = transactions.Count(t => t.Status == "Completed"),
                        totalTransactions = transactions.Count()
                    };
                    
                    return Json(new { success = true, data = summary });
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        data = new { 
                            totalIncome = 0, 
                            totalExpense = 0, 
                            pendingCount = 0,
                            completedCount = 0,
                            totalTransactions = 0
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    error = "Lỗi khi lấy tổng hợp dữ liệu: " + ex.Message,
                    data = new { 
                        totalIncome = 0, 
                        totalExpense = 0, 
                        pendingCount = 0,
                        completedCount = 0,
                        totalTransactions = 0
                    }
                });
            }
        }

        /// <summary>
        /// Tạo Financial Transaction mới
        /// </summary>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateFinancialTransactionDto dto)
        {
            try
            {
                var endpoint = ApiEndpoints.FinancialTransactions.Create;
                var response = await _apiService.PostAsync<FinancialTransactionDto>(endpoint, dto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Tạo phiếu tài chính thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi tạo phiếu tài chính" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi tạo phiếu tài chính: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật Financial Transaction
        /// </summary>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateFinancialTransactionDto dto)
        {
            try
            {
                var endpoint = string.Format(ApiEndpoints.FinancialTransactions.Update, id);
                var response = await _apiService.PutAsync<FinancialTransactionDto>(endpoint, dto);
                
                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data, message = "Cập nhật phiếu tài chính thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi cập nhật phiếu tài chính" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi cập nhật phiếu tài chính: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa Financial Transaction
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
                    return Json(new { success = true, message = "Xóa phiếu tài chính thành công" });
                }
                else
                {
                    return Json(new { success = false, error = response.ErrorMessage ?? "Lỗi khi xóa phiếu tài chính" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi xóa phiếu tài chính: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách Employees cho dropdown
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

        /// <summary>
        /// ✅ 4.3.1.6: Hiển thị trang Sổ Quỹ Tiền Mặt/Ngân Hàng
        /// </summary>
        [HttpGet("CashRegister")]
        public IActionResult CashRegister()
        {
            return View();
        }

        /// <summary>
        /// ✅ 4.3.1.6: Lấy sổ quỹ tiền mặt
        /// </summary>
        [HttpGet("GetCashRegister")]
        public async Task<IActionResult> GetCashRegister(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (fromDate.HasValue)
                {
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }

                var endpoint = ApiEndpoints.FinancialTransactions.GetCashRegister + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<ApiResponse<PaymentMethodRegisterDto>>(endpoint);

                if (response != null && response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data });
                }
                else
                {
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy sổ quỹ tiền mặt" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy sổ quỹ tiền mặt: " + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 4.3.1.6: Lấy sổ quỹ ngân hàng
        /// </summary>
        [HttpGet("GetBankRegister")]
        public async Task<IActionResult> GetBankRegister(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (fromDate.HasValue)
                {
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }

                if (toDate.HasValue)
                {
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }

                var endpoint = ApiEndpoints.FinancialTransactions.GetBankRegister + "?" + string.Join("&", queryParams);
                var response = await _apiService.GetAsync<ApiResponse<PaymentMethodRegisterDto>>(endpoint);

                if (response != null && response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data });
                }
                else
                {
                    return Json(new { success = false, error = response?.ErrorMessage ?? "Lỗi khi lấy sổ quỹ ngân hàng" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Lỗi khi lấy sổ quỹ ngân hàng: " + ex.Message });
            }
        }
    }
}
