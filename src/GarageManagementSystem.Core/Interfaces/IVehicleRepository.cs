using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<Vehicle?> GetVehicleWithCustomerAsync(int id);
        Task<Vehicle?> GetVehicleWithServiceOrdersAsync(int id);
        Task<IEnumerable<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId);
        Task<Vehicle?> GetByLicensePlateAsync(string licensePlate);
        Task<bool> IsLicensePlateExistsAsync(string licensePlate, int? excludeId = null);
        Task<bool> IsVINExistsAsync(string vin, int? excludeId = null);
    }
}
