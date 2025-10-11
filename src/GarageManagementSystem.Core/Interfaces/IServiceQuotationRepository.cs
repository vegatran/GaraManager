using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IServiceQuotationRepository : IGenericRepository<ServiceQuotation>
    {
        Task<ServiceQuotation?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<ServiceQuotation>> GetAllWithDetailsAsync();
        Task<IEnumerable<ServiceQuotation>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<ServiceQuotation>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<ServiceQuotation>> GetByInspectionIdAsync(int inspectionId);
        Task<IEnumerable<ServiceQuotation>> GetByStatusAsync(string status);
        Task<IEnumerable<ServiceQuotation>> GetPendingQuotationsAsync();
        Task<IEnumerable<ServiceQuotation>> GetApprovedQuotationsAsync();
        Task<IEnumerable<ServiceQuotation>> GetExpiredQuotationsAsync();
        Task<string> GenerateQuotationNumberAsync();
    }
}

