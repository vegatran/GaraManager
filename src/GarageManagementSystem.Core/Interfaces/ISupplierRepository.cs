using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<Supplier?> GetBySupplierCodeAsync(string supplierCode);
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
        Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm);
        Task<bool> IsSupplierCodeExistsAsync(string supplierCode, int? excludeId = null);
    }
}

