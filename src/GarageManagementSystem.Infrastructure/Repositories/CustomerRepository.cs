using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetCustomerWithVehiclesAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Vehicles.Where(v => !v.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Customer?> GetCustomerWithServiceOrdersAsync(int id)
        {
            return await _dbSet
                .Include(c => c.ServiceOrders.Where(so => !so.IsDeleted))
                .ThenInclude(so => so.Vehicle)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && 
                    (c.Name.Contains(searchTerm) || 
                     c.Phone!.Contains(searchTerm) || 
                     c.Email!.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<bool> IsPhoneExistsAsync(string phoneNumber, int? excludeId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.Phone == phoneNumber);
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.Email == email);
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
