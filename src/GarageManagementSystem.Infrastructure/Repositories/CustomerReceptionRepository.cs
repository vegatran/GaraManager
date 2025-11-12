using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Core.Enums;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class CustomerReceptionRepository : GenericRepository<CustomerReception>, ICustomerReceptionRepository
    {
        public CustomerReceptionRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CustomerReception>> GetAllWithDetailsAsync()
        {
            return await _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.AssignedTechnician)
                .Include(r => r.VehicleInspection)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public IQueryable<CustomerReception> GetAllWithDetailsQueryable()
        {
            return _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.AssignedTechnician)
                .Include(r => r.VehicleInspection)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .AsQueryable();
        }

        public async Task<CustomerReception?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.AssignedTechnician)
                .Include(r => r.VehicleInspection)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<IEnumerable<CustomerReception>> GetByStatusAsync(ReceptionStatus status)
        {
            return await _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.AssignedTechnician)
                .Where(r => r.Status == status && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerReception>> GetByAssignedTechnicianAsync(int technicianId)
        {
            return await _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Where(r => r.AssignedTechnicianId == technicianId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerReception>> GetPendingReceptionsAsync()
        {
            return await _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.AssignedTechnician)
                .Where(r => r.Status == ReceptionStatus.Pending && !r.IsDeleted)
                .OrderBy(r => r.Priority)
                .ThenByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> IsVehicleInProcessAsync(int vehicleId)
        {
            return await _context.CustomerReceptions
                .AnyAsync(r => r.VehicleId == vehicleId && 
                              !r.IsDeleted && 
                              r.Status != ReceptionStatus.Completed && 
                              r.Status != ReceptionStatus.Cancelled);
        }

        public async Task<CustomerReception?> GetByReceptionNumberAsync(string receptionNumber)
        {
            return await _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.AssignedTechnician)
                .FirstOrDefaultAsync(r => r.ReceptionNumber == receptionNumber && !r.IsDeleted);
        }

        public async Task<bool> UpdateStatusAsync(int id, ReceptionStatus status)
        {
            var reception = await _context.CustomerReceptions
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (reception == null)
                return false;

            reception.Status = status;
            reception.UpdatedDate = DateTime.Now;

            // Cập nhật thời gian tương ứng với trạng thái
            switch (status)
            {
                case ReceptionStatus.Assigned:
                    reception.AssignedDate = DateTime.Now;
                    break;
                case ReceptionStatus.InProgress:
                    reception.InspectionStartDate = DateTime.Now;
                    break;
                case ReceptionStatus.Completed:
                    reception.InspectionCompletedDate = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTechnicianAsync(int id, int technicianId)
        {
            var reception = await _context.CustomerReceptions
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (reception == null)
                return false;

            reception.AssignedTechnicianId = technicianId;
            reception.Status = ReceptionStatus.Assigned;
            reception.AssignedDate = DateTime.Now;
            reception.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<ReceptionStatus, int>> GetStatusStatisticsAsync()
        {
            var statistics = await _context.CustomerReceptions
                .Where(r => !r.IsDeleted)
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return statistics.ToDictionary(s => s.Status, s => s.Count);
        }

        public async Task<IEnumerable<CustomerReception>> GetByPriorityAsync(string priority)
        {
            return await _context.CustomerReceptions
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.AssignedTechnician)
                .Where(r => r.Priority == priority && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
}
