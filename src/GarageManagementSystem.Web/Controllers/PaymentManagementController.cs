using GarageManagementSystem.Shared.DTOs;
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
        /// Lấy danh sách tất cả thanh toán cho DataTable thông qua API
        /// </summary>
        [HttpGet("GetPayments")]
        public async Task<IActionResult> GetPayments()
        {
            var response = await _apiService.GetAsync<List<PaymentTransactionDto>>(ApiEndpoints.PaymentTransactions.GetAll);
            
            if (response.Success)
            {
                var paymentList = new List<object>();
                
                if (response.Data != null)
                {
                    paymentList = response.Data.Select(p => new
                    {
                        id = p.Id,
                        paymentNumber = p.ReceiptNumber,
                        invoiceNumber = p.ServiceOrderId.ToString(),
                        customerName = "N/A", // PaymentTransaction không có Customer trực tiếp
                        amount = p.Amount.ToString("N0"),
                        paymentMethod = p.PaymentMethod,
                        paymentDate = p.PaymentDate,
                        status = p.IsRefund ? "Refund" : "Completed",
                        referenceNumber = p.TransactionReference ?? "N/A",
                        notes = p.Notes ?? "N/A",
                        createdDate = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).Cast<object>().ToList();
                }

                return Json(new { 
                    success = true,
                    data = paymentList,
                    message = "Lấy danh sách thanh toán thành công"
                });
            }
            else
            {
                return Json(new { error = response.ErrorMessage });
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
