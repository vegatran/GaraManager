using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class WarrantyRepository : GenericRepository<Warranty>, IWarrantyRepository
    {
        public WarrantyRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<Warranty?> GetByCodeAsync(string warrantyCode)
        {
            return await _dbSet
                .Include(w => w.Items)
                    .ThenInclude(i => i.Part)
                .Include(w => w.Claims)
                .FirstOrDefaultAsync(w => !w.IsDeleted && w.WarrantyCode == warrantyCode);
        }

        public async Task<Warranty?> GetByServiceOrderIdAsync(int serviceOrderId)
        {
            return await _dbSet
                .Include(w => w.Items)
                    .ThenInclude(i => i.ServiceOrderPart)
                .Include(w => w.Claims)
                .FirstOrDefaultAsync(w => !w.IsDeleted && w.ServiceOrderId == serviceOrderId);
        }

        public async Task<IEnumerable<Warranty>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(w => !w.IsDeleted && w.CustomerId == customerId)
                .Include(w => w.Vehicle)
                .Include(w => w.ServiceOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warranty>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Where(w => !w.IsDeleted && w.VehicleId == vehicleId)
                .Include(w => w.Customer)
                .Include(w => w.ServiceOrder)
                .ToListAsync();
        }
    }
}

