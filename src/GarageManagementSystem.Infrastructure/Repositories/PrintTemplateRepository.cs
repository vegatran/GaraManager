using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation cho PrintTemplate
    /// </summary>
    public class PrintTemplateRepository : GenericRepository<PrintTemplate>, IPrintTemplateRepository
    {
        public PrintTemplateRepository(GarageDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy mẫu mặc định theo loại
        /// </summary>
        public async Task<PrintTemplate?> GetDefaultTemplateAsync(string templateType)
        {
            return await _context.Set<PrintTemplate>()
                .Where(t => t.TemplateType == templateType && 
                           t.IsDefault && 
                           t.IsActive && 
                           !t.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy tất cả mẫu theo loại
        /// </summary>
        public async Task<IEnumerable<PrintTemplate>> GetTemplatesByTypeAsync(string templateType)
        {
            return await _context.Set<PrintTemplate>()
                .Where(t => t.TemplateType == templateType && 
                           !t.IsDeleted)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả mẫu đang hoạt động
        /// </summary>
        public async Task<IEnumerable<PrintTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.Set<PrintTemplate>()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.TemplateType)
                .ThenBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        /// <summary>
        /// Đặt mẫu làm mặc định
        /// </summary>
        public async Task SetAsDefaultAsync(int templateId, string templateType)
        {
            // Bỏ mặc định tất cả mẫu cùng loại
            var existingDefaults = await _context.Set<PrintTemplate>()
                .Where(t => t.TemplateType == templateType && 
                           t.IsDefault && 
                           !t.IsDeleted)
                .ToListAsync();

            foreach (var template in existingDefaults)
            {
                template.IsDefault = false;
                template.UpdatedAt = DateTime.Now;
            }

            // Đặt mẫu mới làm mặc định
            var newDefault = await _context.Set<PrintTemplate>()
                .Where(t => t.Id == templateId && !t.IsDeleted)
                .FirstOrDefaultAsync();

            if (newDefault != null)
            {
                newDefault.IsDefault = true;
                newDefault.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
