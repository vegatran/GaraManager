using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class ServiceOrderRepository : GenericRepository<ServiceOrder>, IServiceOrderRepository
    {
        public ServiceOrderRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<ServiceOrder?> GetServiceOrderWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(so => so.Customer)
                .Include(so => so.Vehicle)
                .Include(so => so.ServiceOrderItems.Where(soi => !soi.IsDeleted))
                .ThenInclude(soi => soi.Service)
                .FirstOrDefaultAsync(so => so.Id == id && !so.IsDeleted);
        }

        public async Task<ServiceOrder?> GetServiceOrderWithCustomerAndVehicleAsync(int id)
        {
            return await _dbSet
                .Include(so => so.Customer)
                .Include(so => so.Vehicle)
                .FirstOrDefaultAsync(so => so.Id == id && !so.IsDeleted);
        }

        public async Task<IEnumerable<ServiceOrder>> GetServiceOrdersByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(so => !so.IsDeleted && so.CustomerId == customerId)
                .Include(so => so.Vehicle)
                .Include(so => so.ServiceOrderItems.Where(soi => !soi.IsDeleted))
                .ThenInclude(soi => soi.Service)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceOrder>> GetServiceOrdersByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Where(so => !so.IsDeleted && so.VehicleId == vehicleId)
                .Include(so => so.Customer)
                .Include(so => so.ServiceOrderItems.Where(soi => !soi.IsDeleted))
                .ThenInclude(soi => soi.Service)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceOrder>> GetServiceOrdersByStatusAsync(string status)
        {
            return await _dbSet
                .Where(so => !so.IsDeleted && so.Status == status)
                .Include(so => so.Customer)
                .Include(so => so.Vehicle)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceOrder>> GetServiceOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(so => !so.IsDeleted && so.OrderDate >= startDate && so.OrderDate <= endDate)
                .Include(so => so.Customer)
                .Include(so => so.Vehicle)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"SO{today:yyyyMMdd}";
            
            var lastOrder = await _dbSet
                .Where(so => so.OrderNumber.StartsWith(prefix))
                .OrderByDescending(so => so.OrderNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastOrder != null)
            {
                var lastSequence = lastOrder.OrderNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }
    }
}
