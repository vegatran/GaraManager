using GarageManagementSystem.Shared.DTOs;

namespace GarageManagementSystem.Shared.Services
{
    /// <summary>
    /// Interface cho service xuất hóa đơn
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Tạo hóa đơn bảo hiểm từ ServiceOrder
        /// </summary>
        Task<InsuranceInvoiceDto> CreateInsuranceInvoiceAsync(int serviceOrderId, InsuranceInvoiceDto invoiceData);
        
        /// <summary>
        /// Xuất hóa đơn bảo hiểm ra file PDF
        /// </summary>
        Task<byte[]> GenerateInsuranceInvoicePdfAsync(int invoiceId);
        
        /// <summary>
        /// Xuất hóa đơn bảo hiểm ra file Excel
        /// </summary>
        Task<byte[]> GenerateInsuranceInvoiceExcelAsync(int invoiceId);
        
        /// <summary>
        /// Lấy danh sách hóa đơn bảo hiểm
        /// </summary>
        Task<List<InsuranceInvoiceDto>> GetInsuranceInvoicesAsync(int? serviceOrderId = null, string? insuranceCompany = null);
        
        /// <summary>
        /// Lấy chi tiết hóa đơn bảo hiểm
        /// </summary>
        Task<InsuranceInvoiceDto?> GetInsuranceInvoiceAsync(int invoiceId);
        
        /// <summary>
        /// Cập nhật hóa đơn bảo hiểm
        /// </summary>
        Task<InsuranceInvoiceDto> UpdateInsuranceInvoiceAsync(int invoiceId, InsuranceInvoiceDto invoiceData);
        
        /// <summary>
        /// Xóa hóa đơn bảo hiểm
        /// </summary>
        Task<bool> DeleteInsuranceInvoiceAsync(int invoiceId);
    }
}
