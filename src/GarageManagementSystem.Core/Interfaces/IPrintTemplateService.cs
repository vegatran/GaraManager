using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Service interface cho Print Template
    /// </summary>
    public interface IPrintTemplateService
    {
        /// <summary>
        /// Lấy mẫu mặc định theo loại
        /// </summary>
        Task<PrintTemplate?> GetDefaultTemplateAsync(string templateType);
        
        /// <summary>
        /// Lấy tất cả mẫu theo loại
        /// </summary>
        Task<IEnumerable<PrintTemplate>> GetTemplatesByTypeAsync(string templateType);
        
        /// <summary>
        /// Lấy tất cả mẫu đang hoạt động
        /// </summary>
        Task<IEnumerable<PrintTemplate>> GetActiveTemplatesAsync();
        
        /// <summary>
        /// Đặt mẫu làm mặc định
        /// </summary>
        Task SetAsDefaultAsync(int templateId, string templateType);
        
        /// <summary>
        /// Tạo mẫu mới
        /// </summary>
        Task<PrintTemplate> CreateTemplateAsync(PrintTemplate template);
        
        /// <summary>
        /// Cập nhật mẫu
        /// </summary>
        Task<PrintTemplate> UpdateTemplateAsync(PrintTemplate template);
        
        /// <summary>
        /// Xóa mẫu
        /// </summary>
        Task DeleteTemplateAsync(int templateId);
        
        /// <summary>
        /// Tạo mẫu mặc định cho báo giá
        /// </summary>
        Task CreateDefaultQuotationTemplateAsync();
    }
}
