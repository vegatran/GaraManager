using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IUnitOfWork unitOfWork,
            ILogger<PaymentController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách payments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPayments(
            [FromQuery] string? paymentMethod = null,
            [FromQuery] int? customerId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var payments = await _unitOfWork.Payments.GetAllAsync();

                // Filters
                if (!string.IsNullOrEmpty(paymentMethod))
                    payments = payments.Where(p => p.PaymentMethod == paymentMethod);

                if (customerId.HasValue)
                    payments = payments.Where(p => p.CustomerId == customerId.Value);

                if (fromDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate >= fromDate.Value);

                if (toDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate <= toDate.Value);

                var result = payments.Select(p => new
                {
                    p.Id,
                    p.PaymentDate,
                    p.CustomerId,
                    p.CustomerName,
                    p.InvoiceId,
                    p.InvoiceNumber,
                    Amount = p.Amount,
                    p.PaymentMethod,
                    p.Status,
                    p.CreatedAt
                }).OrderByDescending(p => p.CreatedAt).ToList();

                return Ok(new { success = true, data = result, count = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách thanh toán" });
            }
        }

        /// <summary>
        /// Lấy chi tiết payment
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thanh toán" });
                }

                var result = new
                {
                    payment.Id,
                    payment.PaymentDate,
                    payment.CustomerId,
                    payment.CustomerName,
                    payment.CustomerPhone,
                    payment.InvoiceId,
                    payment.InvoiceNumber,
                    Amount = payment.Amount,
                    payment.PaymentMethod,
                    payment.ReferenceNumber,
                    payment.Status,
                    payment.Notes,
                    payment.CreatedAt,
                    payment.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin thanh toán" });
            }
        }

        /// <summary>
        /// Tạo payment từ invoice
        /// </summary>
        [HttpPost("from-invoice/{invoiceId}")]
        public async Task<IActionResult> CreateFromInvoice(int invoiceId, [FromBody] CreatePaymentRequest request)
        {
            try
            {
                var invoiceRepo = _unitOfWork.Invoices;
                var allInvoices = await invoiceRepo.GetAllAsync();
                var invoice = allInvoices.FirstOrDefault(i => i.Id == invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                if (invoice.Status == "Paid")
                {
                    return BadRequest(new { success = false, message = "Hóa đơn đã được thanh toán" });
                }

                // Get customer
                var customerRepo = _unitOfWork.Customers;
                var allCustomers = await customerRepo.GetAllAsync();
                var customer = invoice.CustomerId.HasValue ? allCustomers.FirstOrDefault(c => c.Id == invoice.CustomerId.Value) : null;

                var payment = new Payment
                {
                    PaymentDate = DateTime.Now,
                    CustomerId = invoice.CustomerId ?? 0,
                    CustomerName = customer?.Name ?? "",
                    CustomerPhone = customer?.Phone,
                    InvoiceId = invoiceId,
                    InvoiceNumber = invoice.InvoiceNumber,
                    Amount = request.Amount ?? invoice.TotalAmount,
                    PaymentMethod = request.PaymentMethod,
                    ReferenceNumber = request.ReferenceNumber,
                    Status = "Completed",
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Payments.AddAsync(payment);

                // Check if invoice is fully paid
                var allPayments = await _unitOfWork.Payments.GetAllAsync();
                var totalPaid = allPayments
                    .Where(p => p.InvoiceId == invoiceId && p.Status == "Completed")
                    .Sum(p => p.Amount);

                totalPaid += payment.Amount;

                if (totalPaid >= invoice.TotalAmount)
                {
                    invoice.Status = "Paid";
                    invoice.PaidDate = DateTime.Now;
                    invoice.UpdatedAt = DateTime.Now;
                    await _unitOfWork.Invoices.UpdateAsync(invoice);
                }
                else
                {
                    invoice.Status = "Partially Paid";
                    invoice.UpdatedAt = DateTime.Now;
                    await _unitOfWork.Invoices.UpdateAsync(invoice);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created payment for invoice {invoiceId}. Amount: {payment.Amount}, Method: {payment.PaymentMethod}");

                return Ok(new
                {
                    success = true,
                    message = "Thanh toán thành công",
                    data = new
                    {
                        payment.Id,
                        payment.Amount,
                        InvoiceStatus = invoice.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating payment for invoice {invoiceId}");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo thanh toán" });
            }
        }

        /// <summary>
        /// Tạo payment thủ công (không có invoice)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreateManualPaymentRequest request)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                var payment = new Payment
                {
                    PaymentDate = request.PaymentDate ?? DateTime.Now,
                    CustomerId = request.CustomerId,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    ReferenceNumber = request.ReferenceNumber,
                    Status = "Completed",
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created manual payment for customer {customer.Name}. Amount: {payment.Amount}");

                return Ok(new
                {
                    success = true,
                    message = "Tạo thanh toán thành công",
                    data = new
                    {
                        payment.Id,
                        payment.Amount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual payment");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo thanh toán" });
            }
        }

        /// <summary>
        /// Hủy payment (chỉ hủy được trước khi hoàn thành)
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CancelPayment(int id, [FromBody] CancelPaymentRequest request)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thanh toán" });
                }

                if (payment.Status == "Cancelled")
                {
                    return BadRequest(new { success = false, message = "Thanh toán đã bị hủy" });
                }

                payment.Status = "Cancelled";
                payment.Notes = (payment.Notes ?? "") + $"\n[Cancelled] {DateTime.Now:yyyy-MM-dd HH:mm}: {request.Reason}";
                payment.UpdatedAt = DateTime.Now;

                await _unitOfWork.Payments.UpdateAsync(payment);

                // Update invoice status if needed
                if (payment.InvoiceId.HasValue)
                {
                    var invoiceRepo2 = _unitOfWork.Invoices;
                    var allInvoices2 = await invoiceRepo2.GetAllAsync();
                    var invoice = allInvoices2.FirstOrDefault(i => i.Id == payment.InvoiceId.Value);
                    if (invoice != null)
                    {
                        var allPayments = await _unitOfWork.Payments.GetAllAsync();
                        var totalPaid = allPayments
                            .Where(p => p.InvoiceId == payment.InvoiceId.Value && p.Status == "Completed" && p.Id != id)
                            .Sum(p => p.Amount);

                        if (totalPaid >= invoice.TotalAmount)
                        {
                            invoice.Status = "Paid";
                        }
                        else if (totalPaid > 0)
                        {
                            invoice.Status = "Partially Paid";
                        }
                        else
                        {
                            invoice.Status = "Pending";
                            invoice.PaidDate = null;
                        }

                        invoice.UpdatedAt = DateTime.Now;
                        await _unitOfWork.Invoices.UpdateAsync(invoice);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Cancelled payment {id}. Reason: {request.Reason}");

                return Ok(new { success = true, message = "Hủy thanh toán thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling payment {id}");
                return StatusCode(500, new { success = false, message = "Lỗi khi hủy thanh toán" });
            }
        }

        /// <summary>
        /// Thống kê payments theo phương thức
        /// </summary>
        [HttpGet("statistics/by-method")]
        public async Task<IActionResult> GetStatisticsByMethod(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var payments = await _unitOfWork.Payments.GetAllAsync();
                
                // Filter by date
                if (fromDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate >= fromDate.Value);
                
                if (toDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate <= toDate.Value);

                // Filter only completed payments
                payments = payments.Where(p => p.Status == "Completed");

                var statistics = payments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new
                    {
                        PaymentMethod = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(s => s.TotalAmount)
                    .ToList();

                return Ok(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment statistics");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thống kê thanh toán" });
            }
        }

        /// <summary>
        /// Thống kê payments theo ngày
        /// </summary>
        [HttpGet("statistics/by-date")]
        public async Task<IActionResult> GetStatisticsByDate(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var payments = await _unitOfWork.Payments.GetAllAsync();
                
                // Filter by date
                if (fromDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate >= fromDate.Value);
                else
                    payments = payments.Where(p => p.PaymentDate >= DateTime.Now.AddDays(-30));
                
                if (toDate.HasValue)
                    payments = payments.Where(p => p.PaymentDate <= toDate.Value);

                // Filter only completed payments
                payments = payments.Where(p => p.Status == "Completed");

                var statistics = payments
                    .GroupBy(p => p.PaymentDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                return Ok(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment statistics by date");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thống kê thanh toán theo ngày" });
            }
        }

        /// <summary>
        /// Lấy available payment methods từ config
        /// </summary>
        [HttpGet("methods")]
        public IActionResult GetPaymentMethods()
        {
            var methods = new[]
            {
                new { Value = "Cash", Label = "Tiền mặt" },
                new { Value = "Bank Transfer", Label = "Chuyển khoản" },
                new { Value = "Credit Card", Label = "Thẻ tín dụng" },
                new { Value = "E-Wallet", Label = "Ví điện tử" },
                new { Value = "QR Code", Label = "QR Code" },
                new { Value = "Other", Label = "Khác" }
            };

            return Ok(new { success = true, data = methods });
        }
    }

    #region Request Models

    public class CreatePaymentRequest
    {
        public decimal? Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateManualPaymentRequest
    {
        public int CustomerId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }

    public class CancelPaymentRequest
    {
        public string? Reason { get; set; }
    }

    #endregion
}

