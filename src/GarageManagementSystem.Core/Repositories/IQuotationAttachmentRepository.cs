using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    public interface IQuotationAttachmentRepository : IGenericRepository<QuotationAttachment>
    {
        Task<IEnumerable<QuotationAttachment>> GetByQuotationIdAsync(int quotationId);
        Task<IEnumerable<QuotationAttachment>> GetInsuranceDocumentsByQuotationIdAsync(int quotationId);
        Task<QuotationAttachment?> GetByFilePathAsync(string filePath);
    }
}
