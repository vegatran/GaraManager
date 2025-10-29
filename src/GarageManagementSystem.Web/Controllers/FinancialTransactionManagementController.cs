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
    }
}
