using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class ServiceQuotationRepository : GenericRepository<ServiceQuotation>, IServiceQuotationRepository
    {
        public ServiceQuotationRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<ServiceQuotation?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(sq => sq.Customer)
                .Include(sq => sq.Vehicle)
                .Include(sq => sq.VehicleInspection)
                .Include(sq => sq.PreparedBy)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Service)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Part) // ✅ THÊM: Include Part để có PartId
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.InspectionIssue)
                .Include(sq => sq.ServiceOrder)
                .FirstOrDefaultAsync(sq => sq.Id == id && !sq.IsDeleted);
        }

        public async Task<IEnumerable<ServiceQuotation>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted)
                .Include(sq => sq.Customer)
                .Include(sq => sq.Vehicle)
                .Include(sq => sq.PreparedBy)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                .OrderByDescending(sq => sq.QuotationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceQuotation>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted && sq.VehicleId == vehicleId)
                .Include(sq => sq.Customer)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                .OrderByDescending(sq => sq.QuotationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceQuotation>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted && sq.CustomerId == customerId)
                .Include(sq => sq.Vehicle)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                .OrderByDescending(sq => sq.QuotationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceQuotation>> GetByInspectionIdAsync(int inspectionId)
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted && sq.VehicleInspectionId == inspectionId)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                .OrderByDescending(sq => sq.QuotationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceQuotation>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted && sq.Status == status)
                .Include(sq => sq.Customer)
                .Include(sq => sq.Vehicle)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                .OrderByDescending(sq => sq.QuotationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceQuotation>> GetPendingQuotationsAsync()
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted && (sq.Status == "Draft" || sq.Status == "Sent"))
                .Include(sq => sq.Customer)
                .Include(sq => sq.Vehicle)
                .OrderByDescending(sq => sq.QuotationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceQuotation>> GetApprovedQuotationsAsync()
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted && sq.Status == "Approved")
                .Include(sq => sq.Customer)
                .Include(sq => sq.Vehicle)
                .OrderByDescending(sq => sq.ApprovedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceQuotation>> GetExpiredQuotationsAsync()
        {
            var now = DateTime.Now;
            return await _dbSet
                .Where(sq => !sq.IsDeleted && 
                       sq.ValidUntil.HasValue && 
                       sq.ValidUntil.Value < now &&
                       sq.Status == "Sent")
                .Include(sq => sq.Customer)
                .Include(sq => sq.Vehicle)
                .OrderByDescending(sq => sq.QuotationDate)
                .ToListAsync();
        }

        public async Task<string> GenerateQuotationNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"QT{today:yyyyMMdd}";
            
            var lastQuotation = await _dbSet
                .Where(sq => sq.QuotationNumber.StartsWith(prefix))
                .OrderByDescending(sq => sq.QuotationNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastQuotation != null)
            {
                var lastSequence = lastQuotation.QuotationNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }

        public async Task<ServiceQuotation?> GetByVehicleInspectionIdAsync(int vehicleInspectionId)
        {
            return await _dbSet
                .Where(sq => !sq.IsDeleted && sq.VehicleInspectionId == vehicleInspectionId)
                .Include(sq => sq.Customer)
                .Include(sq => sq.Vehicle)
                .Include(sq => sq.VehicleInspection)
                .Include(sq => sq.Items.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync();
        }
    }
}

