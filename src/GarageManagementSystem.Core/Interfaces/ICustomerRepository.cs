using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetCustomerWithVehiclesAsync(int id);
        Task<Customer?> GetCustomerWithServiceOrdersAsync(int id);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, int? excludeId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);
    }
}
