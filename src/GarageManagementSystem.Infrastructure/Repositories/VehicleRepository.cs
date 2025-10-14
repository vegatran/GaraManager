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

        // Additional implementations for API
        public async Task<IEnumerable<Vehicle>> GetAllWithCustomerAsync()
        {
            return await _dbSet
                .Where(v => !v.IsDeleted)
                .Include(v => v.Customer)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByIdWithCustomerAsync(int id)
        {
            return await GetVehicleWithCustomerAsync(id);
        }

        public async Task<IEnumerable<Vehicle>> GetByCustomerIdAsync(int customerId)
        {
            return await GetVehiclesByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Vehicle>> SearchAsync(string searchTerm)
        {
            return await _dbSet
                .Where(v => !v.IsDeleted &&
                    (v.LicensePlate.Contains(searchTerm) ||
                     v.Brand.Contains(searchTerm) ||
                     v.Model.Contains(searchTerm) ||
                     (v.VIN != null && v.VIN.Contains(searchTerm))))
                .Include(v => v.Customer)
                .ToListAsync();
        }

        public async Task<bool> IsVehicleAvailableAsync(int vehicleId)
        {
            // Kiểm tra xe có đang trong quá trình sửa chữa hoặc lịch hẹn chưa hoàn thành không
            var hasActiveServiceOrder = await _context.ServiceOrders
                .AnyAsync(so => !so.IsDeleted && 
                               so.VehicleId == vehicleId && 
                               so.Status != "Completed" && 
                               so.Status != "Cancelled");

            var hasActiveAppointment = await _context.Appointments
                .AnyAsync(a => !a.IsDeleted && 
                              a.VehicleId == vehicleId && 
                              a.Status != "Completed" && 
                              a.Status != "Cancelled" && 
                              a.Status != "NoShow");

            // Xe sẵn sàng nếu không có ServiceOrder hoặc Appointment đang active
            return !hasActiveServiceOrder && !hasActiveAppointment;
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            // Lấy danh sách xe có sẵn (không đang trong quá trình sửa chữa)
            var allVehicles = await _dbSet
                .Where(v => !v.IsDeleted)
                .Include(v => v.Customer)
                .ToListAsync();

            var availableVehicles = new List<Vehicle>();
            
            foreach (var vehicle in allVehicles)
            {
                if (await IsVehicleAvailableAsync(vehicle.Id))
                {
                    availableVehicles.Add(vehicle);
                }
            }

            return availableVehicles;
        }
    }
}
