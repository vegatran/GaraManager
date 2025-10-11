using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class StockTransactionRepository : GenericRepository<StockTransaction>, IStockTransactionRepository
    {
        public StockTransactionRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StockTransaction>> GetByPartIdAsync(int partId)
        {
            return await _dbSet
                .Where(st => !st.IsDeleted && st.PartId == partId)
                .Include(st => st.Supplier)
                .Include(st => st.ProcessedBy)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByServiceOrderIdAsync(int serviceOrderId)
        {
            return await _dbSet
                .Where(st => !st.IsDeleted && st.ServiceOrderId == serviceOrderId)
                .Include(st => st.Part)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetBySupplierIdAsync(int supplierId)
        {
            return await _dbSet
                .Where(st => !st.IsDeleted && st.SupplierId == supplierId)
                .Include(st => st.Part)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByTypeAsync(string transactionType)
        {
            // Convert string to enum for comparison
            var enumType = transactionType switch
            {
                "NhapKho" => StockTransactionType.NhapKho,
                "XuatKho" => StockTransactionType.XuatKho,
                "DieuChinh" => StockTransactionType.DieuChinh,
                "TonDauKy" => StockTransactionType.TonDauKy,
                _ => StockTransactionType.NhapKho
            };

            return await _dbSet
                .Where(st => !st.IsDeleted && st.TransactionType == enumType)
                .Include(st => st.Part)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(st => !st.IsDeleted && st.TransactionDate >= startDate && st.TransactionDate <= endDate)
                .Include(st => st.Part)
                .Include(st => st.Supplier)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<string> GenerateTransactionNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"ST{today:yyyyMMdd}";
            
            var lastTransaction = await _dbSet
                .Where(st => st.TransactionNumber.StartsWith(prefix))
                .OrderByDescending(st => st.TransactionNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastTransaction != null)
            {
                var lastSequence = lastTransaction.TransactionNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }
    }
}

