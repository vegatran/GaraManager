using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department);
        Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(string position);
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
        Task<IEnumerable<Employee>> GetAllWithNavigationAsync();
        Task<Employee?> GetByIdWithNavigationAsync(int id);
    }
}
