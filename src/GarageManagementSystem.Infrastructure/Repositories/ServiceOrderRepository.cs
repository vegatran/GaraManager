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
                .Include(so => so.ServiceOrderItems.Where(soi => !soi.IsDeleted))
                .ThenInclude(soi => soi.AssignedTechnician) // ✅ THÊM: Include AssignedTechnician
                .Include(so => so.ServiceOrderParts.Where(sop => !sop.IsDeleted)) // ✅ THÊM: Include ServiceOrderParts
                .ThenInclude(sop => sop.Part) // ✅ THÊM: Include Part navigation property
                .Include(so => so.ServiceQuotation) // ✅ THÊM: Include ServiceQuotation để lấy quotation gốc
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

        // Additional implementations for API
        public async Task<IEnumerable<ServiceOrder>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Where(so => !so.IsDeleted)
                .Include(so => so.Customer)
                .Include(so => so.Vehicle)
                .Include(so => so.ServiceOrderItems.Where(soi => !soi.IsDeleted))
                .ThenInclude(soi => soi.Service)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
        }

        public async Task<ServiceOrder?> GetByIdWithDetailsAsync(int id)
        {
            return await GetServiceOrderWithDetailsAsync(id);
        }

        public async Task<IEnumerable<ServiceOrder>> GetByCustomerIdAsync(int customerId)
        {
            return await GetServiceOrdersByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<ServiceOrder>> GetByVehicleIdAsync(int vehicleId)
        {
            return await GetServiceOrdersByVehicleIdAsync(vehicleId);
        }

        public async Task<IEnumerable<ServiceOrder>> GetByStatusAsync(string status)
        {
            return await GetServiceOrdersByStatusAsync(status);
        }

        public async Task<ServiceOrder?> GetByServiceQuotationIdAsync(int serviceQuotationId)
        {
            return await _dbSet
                .Where(so => !so.IsDeleted && so.ServiceQuotationId == serviceQuotationId)
                .Include(so => so.Customer)
                .Include(so => so.Vehicle)
                .Include(so => so.ServiceOrderItems.Where(soi => !soi.IsDeleted))
                .FirstOrDefaultAsync();
        }
    }
}
