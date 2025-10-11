using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IStockTransactionRepository : IGenericRepository<StockTransaction>
    {
        Task<IEnumerable<StockTransaction>> GetByPartIdAsync(int partId);
        Task<IEnumerable<StockTransaction>> GetByServiceOrderIdAsync(int serviceOrderId);
        Task<IEnumerable<StockTransaction>> GetBySupplierIdAsync(int supplierId);
        Task<IEnumerable<StockTransaction>> GetByTypeAsync(string transactionType);
        Task<IEnumerable<StockTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<string> GenerateTransactionNumberAsync();
    }
}

