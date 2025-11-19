using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Interface cho service quản lý giao dịch tài chính tự động
    /// </summary>
    public interface IFinancialTransactionService
    {
        /// <summary>
        /// Tự động tạo FinancialTransaction (Income) từ PaymentTransaction (ServiceOrder thanh toán)
        /// </summary>
        /// <param name="paymentTransaction">PaymentTransaction đã được tạo</param>
        /// <returns>FinancialTransaction đã được tạo, null nếu không tạo được</returns>
        Task<FinancialTransaction?> CreateIncomeFromPaymentTransactionAsync(PaymentTransaction paymentTransaction);

        /// <summary>
        /// Tự động tạo FinancialTransaction (Income) từ Payment (Invoice thanh toán)
        /// </summary>
        /// <param name="payment">Payment đã được tạo</param>
        /// <returns>FinancialTransaction đã được tạo, null nếu không tạo được</returns>
        Task<FinancialTransaction?> CreateIncomeFromPaymentAsync(Payment payment);

        /// <summary>
        /// Kiểm tra xem đã có FinancialTransaction cho PaymentTransaction/Payment chưa (tránh duplicate)
        /// </summary>
        /// <param name="relatedEntity">"PaymentTransaction" hoặc "Payment"</param>
        /// <param name="relatedEntityId">ID của PaymentTransaction hoặc Payment</param>
        /// <returns>True nếu đã có, False nếu chưa có</returns>
        Task<bool> HasFinancialTransactionAsync(string relatedEntity, int relatedEntityId);
    }
}

