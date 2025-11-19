using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GarageManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Service quản lý giao dịch tài chính tự động
    /// </summary>
    public class FinancialTransactionService : IFinancialTransactionService
    {
        private readonly GarageDbContext _context;
        private readonly ILogger<FinancialTransactionService> _logger;

        public FinancialTransactionService(GarageDbContext context, ILogger<FinancialTransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tự động tạo FinancialTransaction (Income) từ PaymentTransaction (ServiceOrder thanh toán)
        /// </summary>
        public async Task<FinancialTransaction?> CreateIncomeFromPaymentTransactionAsync(PaymentTransaction paymentTransaction)
        {
            try
            {
                // ✅ Kiểm tra xem đã có FinancialTransaction cho PaymentTransaction này chưa (tránh duplicate)
                var existing = await HasFinancialTransactionAsync("PaymentTransaction", paymentTransaction.Id);
                if (existing)
                {
                    _logger.LogWarning($"FinancialTransaction đã tồn tại cho PaymentTransaction {paymentTransaction.Id}");
                    return null;
                }

                // Lấy ServiceOrder để lấy thông tin
                var serviceOrder = await _context.ServiceOrders
                    .Include(so => so.Customer)
                    .FirstOrDefaultAsync(so => so.Id == paymentTransaction.ServiceOrderId);

                if (serviceOrder == null)
                {
                    _logger.LogWarning($"ServiceOrder {paymentTransaction.ServiceOrderId} không tồn tại");
                    return null;
                }

                // Xác định Category và SubCategory dựa trên ServiceOrder
                string category = "Service Revenue";
                string subCategory = "Service Order";

                // Kiểm tra nếu có parts trong ServiceOrder
                var hasParts = await _context.ServiceOrderParts
                    .AnyAsync(sop => sop.ServiceOrderId == serviceOrder.Id);

                if (hasParts)
                {
                    // Nếu có cả service và parts, có thể tách riêng hoặc gộp chung
                    // Ở đây ta gộp chung vào "Service Revenue"
                    category = "Service Revenue";
                    subCategory = "Service & Parts";
                }

                // ✅ SỬA: Dùng repository method để generate transaction number (tránh race condition)
                // Note: Service này dùng _context riêng, nên cần inject repository hoặc dùng helper method
                // Tạm thời vẫn dùng logic hiện tại nhưng đã có unique index để đảm bảo không duplicate
                var dateStr = DateTime.Now.ToString("yyyyMMdd");
                var count = await _context.FinancialTransactions
                    .CountAsync(ft => !ft.IsDeleted && !string.IsNullOrEmpty(ft.TransactionNumber) && ft.TransactionNumber.StartsWith($"FIN{dateStr}-"));
                var transactionNumber = $"FIN{dateStr}-{(count + 1):D4}";
                
                // ✅ Retry logic nếu duplicate (tránh race condition)
                int retries = 0;
                while (retries < 3)
                {
                    var exists = await _context.FinancialTransactions
                        .AnyAsync(ft => !ft.IsDeleted && ft.TransactionNumber == transactionNumber);
                    
                    if (!exists) break;
                    
                    retries++;
                    count++;
                    transactionNumber = $"FIN{dateStr}-{(count + 1):D4}";
                    
                    if (retries < 3)
                    {
                        await Task.Delay(10); // Wait 10ms before retry
                    }
                }

                // Tạo FinancialTransaction
                var financialTransaction = new FinancialTransaction
                {
                    TransactionNumber = transactionNumber,
                    TransactionType = paymentTransaction.IsRefund ? "Expense" : "Income", // Refund là Expense
                    Category = category,
                    SubCategory = subCategory,
                    Amount = paymentTransaction.Amount,
                    Currency = "VND",
                    TransactionDate = paymentTransaction.PaymentDate,
                    PaymentMethod = paymentTransaction.PaymentMethod, // Cash, Bank Transfer, Credit Card, etc.
                    ReferenceNumber = paymentTransaction.ReceiptNumber,
                    Description = paymentTransaction.IsRefund 
                        ? $"Hoàn tiền cho ServiceOrder: {serviceOrder.OrderNumber}"
                        : $"Thu tiền từ ServiceOrder: {serviceOrder.OrderNumber} - Khách hàng: {serviceOrder.Customer?.Name ?? "N/A"}",
                    RelatedEntity = "PaymentTransaction",
                    RelatedEntityId = paymentTransaction.Id,
                    EmployeeId = paymentTransaction.ReceivedById,
                    Status = "Completed", // Thanh toán đã hoàn tất
                    IsApproved = true, // Tự động approved cho thanh toán
                    IsReconciled = false, // Chưa đối soát
                    Notes = paymentTransaction.Notes ?? (paymentTransaction.IsRefund ? paymentTransaction.RefundReason : null),
                    CreatedAt = DateTime.Now
                };

                await _context.FinancialTransactions.AddAsync(financialTransaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Tạo FinancialTransaction (Income) {transactionNumber} từ PaymentTransaction {paymentTransaction.Id} - Amount: {paymentTransaction.Amount}");
                
                return financialTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tạo FinancialTransaction từ PaymentTransaction {paymentTransaction.Id}");
                return null; // Không throw exception để không ảnh hưởng đến PaymentTransaction creation
            }
        }

        /// <summary>
        /// Tự động tạo FinancialTransaction (Income) từ Payment (Invoice thanh toán)
        /// </summary>
        public async Task<FinancialTransaction?> CreateIncomeFromPaymentAsync(Payment payment)
        {
            try
            {
                // ✅ Kiểm tra xem đã có FinancialTransaction cho Payment này chưa (tránh duplicate)
                var existing = await HasFinancialTransactionAsync("Payment", payment.Id);
                if (existing)
                {
                    _logger.LogWarning($"FinancialTransaction đã tồn tại cho Payment {payment.Id}");
                    return null;
                }

                // Lấy Invoice để lấy thông tin (nếu có)
                Invoice? invoice = null;
                if (payment.InvoiceId.HasValue)
                {
                    invoice = await _context.Invoices
                        .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId.Value);
                }

                // Xác định Category và SubCategory
                string category = "Service Revenue";
                string subCategory = "Invoice Payment";

                if (invoice != null)
                {
                    // Nếu có Invoice, có thể xác định chính xác hơn
                    if (!string.IsNullOrEmpty(invoice.InsuranceCompany))
                    {
                        category = "Insurance Revenue";
                        subCategory = "Insurance Invoice";
                    }
                    else
                    {
                        category = "Service Revenue";
                        subCategory = "Invoice Payment";
                    }
                }

                // ✅ SỬA: Dùng repository method để generate transaction number (tránh race condition)
                // Note: Service này dùng _context riêng, nên cần inject repository hoặc dùng helper method
                // Tạm thời vẫn dùng logic hiện tại nhưng đã có unique index để đảm bảo không duplicate
                var dateStr = DateTime.Now.ToString("yyyyMMdd");
                var count = await _context.FinancialTransactions
                    .CountAsync(ft => !ft.IsDeleted && !string.IsNullOrEmpty(ft.TransactionNumber) && ft.TransactionNumber.StartsWith($"FIN{dateStr}-"));
                var transactionNumber = $"FIN{dateStr}-{(count + 1):D4}";
                
                // ✅ Retry logic nếu duplicate (tránh race condition)
                int retries = 0;
                while (retries < 3)
                {
                    var exists = await _context.FinancialTransactions
                        .AnyAsync(ft => !ft.IsDeleted && ft.TransactionNumber == transactionNumber);
                    
                    if (!exists) break;
                    
                    retries++;
                    count++;
                    transactionNumber = $"FIN{dateStr}-{(count + 1):D4}";
                    
                    if (retries < 3)
                    {
                        await Task.Delay(10); // Wait 10ms before retry
                    }
                }

                // Tạo FinancialTransaction
                var financialTransaction = new FinancialTransaction
                {
                    TransactionNumber = transactionNumber,
                    TransactionType = payment.Status == "Cancelled" ? "Expense" : "Income", // Cancelled payment là Expense (refund)
                    Category = category,
                    SubCategory = subCategory,
                    Amount = payment.Amount,
                    Currency = "VND",
                    TransactionDate = payment.PaymentDate,
                    PaymentMethod = payment.PaymentMethod, // Cash, Bank Transfer, Credit Card, etc.
                    ReferenceNumber = invoice != null ? invoice.InvoiceNumber : payment.ReferenceNumber,
                    Description = invoice != null
                        ? $"Thu tiền từ Invoice: {invoice.InvoiceNumber} - Khách hàng: {payment.CustomerName}"
                        : $"Thu tiền từ Khách hàng: {payment.CustomerName}",
                    RelatedEntity = "Payment",
                    RelatedEntityId = payment.Id,
                    EmployeeId = null, // Payment không có EmployeeId trực tiếp
                    Status = payment.Status == "Completed" ? "Completed" : "Pending",
                    IsApproved = payment.Status == "Completed",
                    IsReconciled = false, // Chưa đối soát
                    Notes = payment.Notes,
                    CreatedAt = DateTime.Now
                };

                await _context.FinancialTransactions.AddAsync(financialTransaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Tạo FinancialTransaction (Income) {transactionNumber} từ Payment {payment.Id} - Amount: {payment.Amount}");
                
                return financialTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tạo FinancialTransaction từ Payment {payment.Id}");
                return null; // Không throw exception để không ảnh hưởng đến Payment creation
            }
        }

        /// <summary>
        /// Kiểm tra xem đã có FinancialTransaction cho PaymentTransaction/Payment chưa (tránh duplicate)
        /// </summary>
        public async Task<bool> HasFinancialTransactionAsync(string relatedEntity, int relatedEntityId)
        {
            try
            {
                return await _context.FinancialTransactions
                    .AnyAsync(ft => !ft.IsDeleted 
                        && ft.RelatedEntity == relatedEntity 
                        && ft.RelatedEntityId == relatedEntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi kiểm tra FinancialTransaction cho {relatedEntity} {relatedEntityId}");
                return false; // Nếu có lỗi, trả về false để tiếp tục tạo
            }
        }
    }
}

