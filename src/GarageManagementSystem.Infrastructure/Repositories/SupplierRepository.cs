using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<Supplier?> GetBySupplierCodeAsync(string supplierCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => !s.IsDeleted && s.SupplierCode == supplierCode);
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted &&
                    (s.SupplierName.Contains(searchTerm) ||
                     s.SupplierCode.Contains(searchTerm) ||
                     (s.Phone != null && s.Phone.Contains(searchTerm))))
                .ToListAsync();
        }

        public async Task<bool> IsSupplierCodeExistsAsync(string supplierCode, int? excludeId = null)
        {
            var query = _dbSet.Where(s => !s.IsDeleted && s.SupplierCode == supplierCode);
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}

