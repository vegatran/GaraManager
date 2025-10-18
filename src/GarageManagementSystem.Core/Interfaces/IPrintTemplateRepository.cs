using GarageManagementSystem.Core.Entities;

namespace GarageManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Repository interface cho PrintTemplate
    /// </summary>
    public interface IPrintTemplateRepository : IGenericRepository<PrintTemplate>
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
    }
}
