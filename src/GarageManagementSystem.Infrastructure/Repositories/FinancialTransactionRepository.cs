using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho FinancialTransaction với method generate transaction number
    /// </summary>
    public class FinancialTransactionRepository : GenericRepository<FinancialTransaction>, IFinancialTransactionRepository
    {
        public FinancialTransactionRepository(GarageDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Generate unique transaction number (FIN-yyyyMMdd-####)
        /// </summary>
        public async Task<string> GenerateTransactionNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"FIN{today:yyyyMMdd}";
            
            // ✅ SỬA: Query với filter IsDeleted và null check
            var lastTransaction = await _dbSet
                .Where(ft => !ft.IsDeleted 
                    && !string.IsNullOrEmpty(ft.TransactionNumber) 
                    && ft.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(ft => ft.TransactionNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastTransaction != null && !string.IsNullOrEmpty(lastTransaction.TransactionNumber))
            {
                // Extract sequence from transaction number (prefix length + 1 for "-")
                var lastSequenceStr = lastTransaction.TransactionNumber.Substring(prefix.Length + 1);
                if (int.TryParse(lastSequenceStr, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}-{sequence:D4}";
        }
    }
}

