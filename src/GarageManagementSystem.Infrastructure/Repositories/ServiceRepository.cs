using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Service>> GetActiveServicesAsync()
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetServicesByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive && s.Category == category)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> SearchServicesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive && 
                    (s.Name.Contains(searchTerm) || 
                     s.Description!.Contains(searchTerm) ||
                     s.Category!.Contains(searchTerm)))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}
