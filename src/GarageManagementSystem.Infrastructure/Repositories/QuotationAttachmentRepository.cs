using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;
using GarageManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.Infrastructure.Repositories
{
    public class QuotationAttachmentRepository : GenericRepository<QuotationAttachment>, IQuotationAttachmentRepository
    {
        public QuotationAttachmentRepository(GarageDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<QuotationAttachment>> GetByQuotationIdAsync(int quotationId)
        {
            return await _context.QuotationAttachments
                .Where(a => a.ServiceQuotationId == quotationId)
                .Include(a => a.UploadedBy)
                .OrderByDescending(a => a.UploadedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuotationAttachment>> GetInsuranceDocumentsByQuotationIdAsync(int quotationId)
        {
            return await _context.QuotationAttachments
                .Where(a => a.ServiceQuotationId == quotationId && a.IsInsuranceDocument)
                .Include(a => a.UploadedBy)
                .OrderByDescending(a => a.UploadedDate)
                .ToListAsync();
        }

        public async Task<QuotationAttachment?> GetByFilePathAsync(string filePath)
        {
            return await _context.QuotationAttachments
                .FirstOrDefaultAsync(a => a.FilePath == filePath);
        }
    }
}
