using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IVehicleInspectionRepository : IGenericRepository<VehicleInspection>
    {
        Task<VehicleInspection?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<VehicleInspection>> GetAllWithDetailsAsync();
        Task<IEnumerable<VehicleInspection>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<VehicleInspection>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<VehicleInspection>> GetByInspectorIdAsync(int inspectorId);
        Task<IEnumerable<VehicleInspection>> GetByStatusAsync(string status);
        Task<IEnumerable<VehicleInspection>> GetPendingInspectionsAsync();
        Task<string> GenerateInspectionNumberAsync();
        Task<VehicleInspection?> GetLatestInspectionByVehicleAsync(int vehicleId);
    }
}

