using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IPartRepository : IGenericRepository<Part>
    {
        Task<Part?> GetByPartNumberAsync(string partNumber);
        Task<IEnumerable<Part>> GetByCategoryAsync(string category);
        Task<IEnumerable<Part>> GetLowStockPartsAsync();
        Task<IEnumerable<Part>> SearchPartsAsync(string searchTerm);
        Task<bool> IsPartNumberExistsAsync(string partNumber, int? excludeId = null);
        Task UpdateStockAsync(int partId, int quantity);
    }
}

