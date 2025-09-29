using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IServiceRepository : IGenericRepository<Service>
    {
        Task<IEnumerable<Service>> GetActiveServicesAsync();
        Task<IEnumerable<Service>> GetServicesByCategoryAsync(string category);
        Task<IEnumerable<Service>> SearchServicesAsync(string searchTerm);
    }
}
