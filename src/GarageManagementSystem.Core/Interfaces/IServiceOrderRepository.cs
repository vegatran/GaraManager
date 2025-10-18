using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IServiceOrderRepository : IGenericRepository<ServiceOrder>
    {
        Task<ServiceOrder?> GetServiceOrderWithDetailsAsync(int id);
        Task<ServiceOrder?> GetServiceOrderWithCustomerAndVehicleAsync(int id);
        Task<IEnumerable<ServiceOrder>> GetServiceOrdersByCustomerIdAsync(int customerId);
        Task<IEnumerable<ServiceOrder>> GetServiceOrdersByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<ServiceOrder>> GetServiceOrdersByStatusAsync(string status);
        Task<IEnumerable<ServiceOrder>> GetServiceOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<string> GenerateOrderNumberAsync();
        
        // Additional methods for API
        Task<IEnumerable<ServiceOrder>> GetAllWithDetailsAsync();
        Task<ServiceOrder?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<ServiceOrder>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<ServiceOrder>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<ServiceOrder>> GetByStatusAsync(string status);
        Task<ServiceOrder?> GetByServiceQuotationIdAsync(int serviceQuotationId);
    }
}
