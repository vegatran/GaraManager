using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByServiceOrderIdAsync(int serviceOrderId)
        {
            return await _dbSet
                .Where(pt => !pt.IsDeleted && pt.ServiceOrderId == serviceOrderId)
                .Include(pt => pt.ReceivedBy)
                .OrderBy(pt => pt.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(pt => !pt.IsDeleted && pt.PaymentDate >= startDate && pt.PaymentDate <= endDate)
                .Include(pt => pt.ServiceOrder)
                .Include(pt => pt.ReceivedBy)
                .OrderByDescending(pt => pt.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentTransaction>> GetByPaymentMethodAsync(string paymentMethod)
        {
            return await _dbSet
                .Where(pt => !pt.IsDeleted && pt.PaymentMethod == paymentMethod)
                .Include(pt => pt.ServiceOrder)
                .OrderByDescending(pt => pt.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaidForOrderAsync(int serviceOrderId)
        {
            return await _dbSet
                .Where(pt => !pt.IsDeleted && pt.ServiceOrderId == serviceOrderId && !pt.IsRefund)
                .SumAsync(pt => pt.Amount);
        }

        public async Task<string> GenerateReceiptNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"PT{today:yyyyMMdd}";
            
            var lastPayment = await _dbSet
                .Where(pt => pt.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(pt => pt.ReceiptNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastPayment != null)
            {
                var lastSequence = lastPayment.ReceiptNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }
    }
}

