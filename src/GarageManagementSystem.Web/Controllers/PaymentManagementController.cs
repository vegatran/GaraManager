using AutoMapper;
using GarageManagementSystem.Shared.DTOs;
using GarageManagementSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Web.Services;
using GarageManagementSystem.Web.Configuration;

namespace GarageManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller quản lý thanh toán với đầy đủ các thao tác CRUD thông qua API
    /// </summary>
    [Authorize]
    [Route("PaymentManagement")]
    public class PaymentManagementController : Controller
    {
        private readonly ApiService _apiService;

        public PaymentManagementController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Hiển thị trang quản lý thanh toán
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lấy danh sách thanh toán với pagination theo pattern chung
        /// </summary>
        [HttpGet("GetPayments")]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? paymentMethod = null,
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
                
                if (!string.IsNullOrEmpty(searchTerm))
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
                if (!string.IsNullOrEmpty(paymentMethod))
                    queryParams.Add($"paymentMethod={Uri.EscapeDataString(paymentMethod)}");
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var queryString = string.Join("&", queryParams);
                var endpoint = $"{ApiEndpoints.PaymentTransactions.GetAll}?{queryString}";

                // Use PagedResponse pattern like EmployeeManagement
                var response = await _apiService.GetAsync<PagedResponse<PaymentTransactionDto>>(endpoint);
                
                if (response.Success && response.Data != null)
                {
                    var paymentList = response.Data.Data.Select(p => new
                    {
                        id = p.Id,
                        paymentNumber = p.ReceiptNumber,
                        invoiceNumber = p.ServiceOrderId != null ? p.ServiceOrderId.ToString() : "N/A",
                        customerName = "N/A",
                        amount = p.Amount,
                        paymentMethod = p.PaymentMethod ?? "N/A",
                        paymentDate = p.PaymentDate.ToString("yyyy-MM-dd"),
                        status = p.IsRefund ? "Refund" : "Completed",
                        referenceNumber = p.TransactionReference ?? "N/A",
                        notes = p.Notes ?? "N/A"
                    }).ToList();

                    return Json(new { 
                        success = true,
                        data = paymentList,
                        totalCount = response.Data.TotalCount,
                        message = "Lấy danh sách thanh toán thành công"
                    });
                }
                else
                {
                    return Json(new { 
                        success = false,
                        data = new List<object>(),
                        totalCount = 0,
                        error = response.ErrorMessage ?? "Lỗi khi lấy danh sách thanh toán"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false,
                    data = new List<object>(),
                    totalCount = 0,
                    error = "Lỗi khi lấy danh sách thanh toán: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết thanh toán theo ID thông qua API
        /// </summary>
        [HttpGet("GetPayment/{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var response = await _apiService.GetAsync<PaymentTransactionDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.PaymentTransactions.GetById, id)
            );
            
            return Json(response);
        }

        /// <summary>
        /// Tạo thanh toán mới thông qua API
        /// </summary>
        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentTransactionDto paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PostAsync<PaymentTransactionDto>(
                ApiEndpoints.PaymentTransactions.Create,
                paymentDto
            );

            return Json(response);
        }

        /// <summary>
        /// Cập nhật thông tin thanh toán thông qua API
        /// </summary>
        [HttpPut("UpdatePayment/{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] CreatePaymentTransactionDto paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errorMessage = "Dữ liệu không hợp lệ" });
            }

            var response = await _apiService.PutAsync<PaymentTransactionDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.PaymentTransactions.Update, id),
                paymentDto
            );

            return Json(response);
        }

        /// <summary>
        /// Xóa thanh toán thông qua API
        /// </summary>
        [HttpDelete("DeletePayment/{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var response = await _apiService.DeleteAsync<PaymentTransactionDto>(
                ApiEndpoints.Builder.WithId(ApiEndpoints.PaymentTransactions.Delete, id)
            );

            return Json(response);
        }

        /// <summary>
        /// Lấy danh sách khách hàng có sẵn cho dropdown
        /// </summary>
        [HttpGet("GetAvailableCustomers")]
        public async Task<IActionResult> GetAvailableCustomers()
        {
            var response = await _apiService.GetAsync<List<CustomerDto>>(ApiEndpoints.Customers.GetAll);
            
            if (response.Success && response.Data != null)
            {
                var customers = response.Data.Select(c => new
                {
                    value = c.Id.ToString(),
                    text = c.Name
                }).Cast<object>().ToList();
                
                return Json(customers);
            }

            return Json(new List<object>());
        }

    }
}
