using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class PartRepository : GenericRepository<Part>, IPartRepository
    {
        public PartRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<Part?> GetByPartNumberAsync(string partNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.PartNumber == partNumber);
        }

        public async Task<IEnumerable<Part>> GetByCategoryAsync(string category)
        {
            return await _dbSet
                .Include(p => p.PartUnits.Where(u => !u.IsDeleted))
                .Where(p => !p.IsDeleted && p.Category == category)
                .OrderBy(p => p.PartName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Part>> GetLowStockPartsAsync()
        {
            return await _dbSet
                .Include(p => p.PartUnits.Where(u => !u.IsDeleted))
                .Where(p => !p.IsDeleted && p.IsActive && p.QuantityInStock <= p.MinimumStock)
                .OrderBy(p => p.QuantityInStock)
                .ToListAsync();
        }

        public async Task<IEnumerable<Part>> SearchPartsAsync(string searchTerm)
        {
            return await _dbSet
                .Include(p => p.PartUnits.Where(u => !u.IsDeleted))
                .Where(p => !p.IsDeleted && 
                    (p.PartNumber.Contains(searchTerm) ||
                     p.PartName.Contains(searchTerm) ||
                     (p.Brand != null && p.Brand.Contains(searchTerm))))
                .ToListAsync();
        }

        public async Task<bool> IsPartNumberExistsAsync(string partNumber, int? excludeId = null)
        {
            var query = _dbSet.Where(p => !p.IsDeleted && p.PartNumber == partNumber);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task UpdateStockAsync(int partId, int quantity)
        {
            var part = await GetByIdAsync(partId);
            if (part != null)
            {
                part.QuantityInStock += quantity;
                await UpdateAsync(part);
            }
        }

        /// <summary>
        /// ✅ THÊM: Bulk load parts by IDs để tối ưu performance
        /// </summary>
        public async Task<IEnumerable<Part>> GetByIdsAsync(List<int> ids)
        {
            return await _dbSet
                .Include(p => p.PartUnits.Where(u => !u.IsDeleted))
                .Where(p => !p.IsDeleted && ids.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<Part?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.PartUnits.Where(u => !u.IsDeleted))
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);
        }
    }
}

