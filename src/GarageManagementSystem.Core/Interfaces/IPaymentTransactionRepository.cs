using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction>
    {
        Task<IEnumerable<PaymentTransaction>> GetByServiceOrderIdAsync(int serviceOrderId);
        Task<IEnumerable<PaymentTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentTransaction>> GetByPaymentMethodAsync(string paymentMethod);
        Task<decimal> GetTotalPaidForOrderAsync(int serviceOrderId);
        Task<string> GenerateReceiptNumberAsync();
    }
}

