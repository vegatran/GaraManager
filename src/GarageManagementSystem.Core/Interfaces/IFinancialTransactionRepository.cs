using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Repository interface cho FinancialTransaction với method generate transaction number
    /// </summary>
    public interface IFinancialTransactionRepository : IGenericRepository<FinancialTransaction>
    {
        /// <summary>
        /// Generate unique transaction number
        /// </summary>
        Task<string> GenerateTransactionNumberAsync();
    }
}

