using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            return await _dbSet
                .Where(e => !e.IsDeleted && e.Status == "Active")
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department)
        {
            return await _dbSet
                .Where(e => !e.IsDeleted && e.Department == department)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPositionAsync(string position)
        {
            return await _dbSet
                .Where(e => !e.IsDeleted && e.Position == position)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(e => !e.IsDeleted && 
                    (e.Name.Contains(searchTerm) || 
                     e.Phone!.Contains(searchTerm) || 
                     e.Email!.Contains(searchTerm) ||
                     e.Position!.Contains(searchTerm) ||
                     e.Department!.Contains(searchTerm)))
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllWithNavigationAsync()
        {
            return await _dbSet
                .Where(e => !e.IsDeleted)
                .Include(e => e.PositionNavigation)
                .Include(e => e.DepartmentNavigation)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdWithNavigationAsync(int id)
        {
            return await _dbSet
                .Where(e => e.Id == id && !e.IsDeleted)
                .Include(e => e.PositionNavigation)
                .Include(e => e.DepartmentNavigation)
                .FirstOrDefaultAsync();
        }
    }
}
