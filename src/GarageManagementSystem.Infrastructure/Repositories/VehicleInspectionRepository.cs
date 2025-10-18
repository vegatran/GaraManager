using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class VehicleInspectionRepository : GenericRepository<VehicleInspection>, IVehicleInspectionRepository
    {
        public VehicleInspectionRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<VehicleInspection?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(vi => vi.Customer)
                .Include(vi => vi.Vehicle)
                .Include(vi => vi.Inspector)
                .Include(vi => vi.Issues.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.SuggestedService)
                .Include(vi => vi.Photos.Where(p => !p.IsDeleted))
                .Include(vi => vi.Quotation)
                .FirstOrDefaultAsync(vi => vi.Id == id && !vi.IsDeleted);
        }

        public async Task<IEnumerable<VehicleInspection>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Where(vi => !vi.IsDeleted)
                .Include(vi => vi.Customer)
                .Include(vi => vi.Vehicle)
                .Include(vi => vi.Inspector)
                .Include(vi => vi.Issues.Where(i => !i.IsDeleted))
                .Include(vi => vi.Photos.Where(p => !p.IsDeleted))
                .OrderByDescending(vi => vi.InspectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleInspection>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Where(vi => !vi.IsDeleted && vi.VehicleId == vehicleId)
                .Include(vi => vi.Inspector)
                .Include(vi => vi.Issues.Where(i => !i.IsDeleted))
                .OrderByDescending(vi => vi.InspectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleInspection>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(vi => !vi.IsDeleted && vi.CustomerId == customerId)
                .Include(vi => vi.Vehicle)
                .Include(vi => vi.Inspector)
                .OrderByDescending(vi => vi.InspectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleInspection>> GetByInspectorIdAsync(int inspectorId)
        {
            return await _dbSet
                .Where(vi => !vi.IsDeleted && vi.InspectorId == inspectorId)
                .Include(vi => vi.Vehicle)
                .Include(vi => vi.Customer)
                .OrderByDescending(vi => vi.InspectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleInspection>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(vi => !vi.IsDeleted && vi.Status == status)
                .Include(vi => vi.Customer)
                .Include(vi => vi.Vehicle)
                .Include(vi => vi.Inspector)
                .OrderByDescending(vi => vi.InspectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleInspection>> GetPendingInspectionsAsync()
        {
            return await GetByStatusAsync("Pending");
        }

        public async Task<string> GenerateInspectionNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"INS{today:yyyyMMdd}";
            
            var lastInspection = await _dbSet
                .Where(vi => vi.InspectionNumber.StartsWith(prefix))
                .OrderByDescending(vi => vi.InspectionNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastInspection != null)
            {
                var lastSequence = lastInspection.InspectionNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }

        public async Task<VehicleInspection?> GetLatestInspectionByVehicleAsync(int vehicleId)
        {
            return await _dbSet
                .Where(vi => !vi.IsDeleted && vi.VehicleId == vehicleId)
                .Include(vi => vi.Issues.Where(i => !i.IsDeleted))
                .OrderByDescending(vi => vi.InspectionDate)
                .FirstOrDefaultAsync();
        }

        public async Task<VehicleInspection?> GetByCustomerReceptionIdAsync(int customerReceptionId)
        {
            return await _dbSet
                .Where(vi => !vi.IsDeleted && vi.CustomerReceptionId == customerReceptionId)
                .Include(vi => vi.Customer)
                .Include(vi => vi.Vehicle)
                .Include(vi => vi.Inspector)
                .Include(vi => vi.Issues.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync();
        }
    }
}

