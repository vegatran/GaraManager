using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<Vehicle?> GetVehicleWithCustomerAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<Vehicle?> GetVehicleWithServiceOrdersAsync(int id)
        {
            return await _dbSet
                .Include(v => v.ServiceOrders.Where(so => !so.IsDeleted))
                .ThenInclude(so => so.Customer)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(v => !v.IsDeleted && v.CustomerId == customerId)
                .Include(v => v.Customer)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate)
        {
            return await _dbSet
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => !v.IsDeleted && v.LicensePlate == licensePlate);
        }

        public async Task<bool> IsLicensePlateExistsAsync(string licensePlate, int? excludeId = null)
        {
            var query = _dbSet.Where(v => !v.IsDeleted && v.LicensePlate == licensePlate);
            
            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsVINExistsAsync(string vin, int? excludeId = null)
        {
            if (string.IsNullOrEmpty(vin))
                return false;

            var query = _dbSet.Where(v => !v.IsDeleted && v.VIN == vin);
            
            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
