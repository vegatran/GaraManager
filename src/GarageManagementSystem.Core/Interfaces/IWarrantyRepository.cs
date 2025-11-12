using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IWarrantyRepository : IGenericRepository<Warranty>
    {
        Task<Warranty?> GetByCodeAsync(string warrantyCode);
        Task<Warranty?> GetByServiceOrderIdAsync(int serviceOrderId);
        Task<IEnumerable<Warranty>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Warranty>> GetByVehicleIdAsync(int vehicleId);
    }
}

