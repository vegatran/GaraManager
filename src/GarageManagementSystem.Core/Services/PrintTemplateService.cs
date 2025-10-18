using GarageManagementSystem.Core.Entities;
using GarageManagementSystem.Core.Interfaces;

namespace GarageManagementSystem.Core.Services
{
    /// <summary>
    /// Service cho quản lý Print Template
    /// </summary>
    public class PrintTemplateService : IPrintTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrintTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy mẫu mặc định theo loại
        /// </summary>
        public async Task<PrintTemplate?> GetDefaultTemplateAsync(string templateType)
        {
            return await _unitOfWork.PrintTemplates.GetDefaultTemplateAsync(templateType);
        }

        /// <summary>
        /// Lấy tất cả mẫu theo loại
        /// </summary>
        public async Task<IEnumerable<PrintTemplate>> GetTemplatesByTypeAsync(string templateType)
        {
            return await _unitOfWork.PrintTemplates.GetTemplatesByTypeAsync(templateType);
        }

        /// <summary>
        /// Lấy tất cả mẫu đang hoạt động
        /// </summary>
        public async Task<IEnumerable<PrintTemplate>> GetActiveTemplatesAsync()
        {
            return await _unitOfWork.PrintTemplates.GetActiveTemplatesAsync();
        }

        /// <summary>
        /// Đặt mẫu làm mặc định
        /// </summary>
        public async Task SetAsDefaultAsync(int templateId, string templateType)
        {
            await _unitOfWork.PrintTemplates.SetAsDefaultAsync(templateId, templateType);
        }

        /// <summary>
        /// Tạo mẫu mới
        /// </summary>
        public async Task<PrintTemplate> CreateTemplateAsync(PrintTemplate template)
        {
            var repository = _unitOfWork.Repository<PrintTemplate>();
            await repository.AddAsync(template);
            await _unitOfWork.SaveChangesAsync();
            return template;
        }

        /// <summary>
        /// Cập nhật mẫu
        /// </summary>
        public async Task<PrintTemplate> UpdateTemplateAsync(PrintTemplate template)
        {
            var repository = _unitOfWork.Repository<PrintTemplate>();
            await repository.UpdateAsync(template);
            await _unitOfWork.SaveChangesAsync();
            return template;
        }

        /// <summary>
        /// Xóa mẫu (soft delete)
        /// </summary>
        public async Task DeleteTemplateAsync(int templateId)
        {
            var repository = _unitOfWork.Repository<PrintTemplate>();
            var template = await repository.GetByIdAsync(templateId);
            if (template != null)
            {
                await repository.DeleteAsync(template);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Tạo mẫu mặc định cho báo giá
        /// </summary>
        public async Task CreateDefaultQuotationTemplateAsync()
        {
            var existingDefault = await GetDefaultTemplateAsync("Quotation");
            if (existingDefault != null) return;

            var defaultTemplate = new PrintTemplate
            {
                TemplateName = "Mẫu Báo Giá Mặc Định",
                TemplateType = "Quotation",
                Description = "Mẫu báo giá chuẩn cho gara ô tô",
                IsDefault = true,
                IsActive = true,
                DisplayOrder = 1,
                CompanyInfo = @"{
                    ""companyName"": ""GARAGE Ô TÔ THUẬN PHÁT"",
                    ""companyAddress"": ""313 Võ Văn Vân, Vĩnh Lộc B, H. Bình Chánh, TP.HCM"",
                    ""companyPhone"": ""032.7007.985 (Mr. Bằng)"",
                    ""companyEmail"": ""garage@thuanphat.com"",
                    ""companyWebsite"": ""www.thuanphat.com"",
                    ""taxCode"": ""0123456789""
                }",
                HeaderHtml = @"<div class=""header"">
                    <div class=""logo"">
                        <img src=""/images/logo.png"" alt=""Logo"" style=""max-height: 80px;"">
                    </div>s
                    <div class=""company-info"">
                        <h1 id=""companyName"">GARAGE Ô TÔ THUẬN PHÁT</h1>
                        <p id=""companyAddress"">313 Võ Văn Vân, Vĩnh Lộc B, H. Bình Chánh, TP.HCM</p>
                        <p id=""companyPhone"">032.7007.985 (Mr. Bằng)</p>
                        <p id=""companyEmail"">garage@thuanphat.com</p>
                    </div>
                </div>",
                FooterHtml = @"<div class=""footer"">
                    <p>Xin cảm ơn quý khách đã tin tưởng dịch vụ của chúng tôi!</p>
                    <p>Báo giá có hiệu lực trong vòng 7 ngày kể từ ngày phát hành.</p>
                </div>",
                CustomCss = @"@@media print {
                    body { font-size: 11px; }
                    .print-container { margin: 0; padding: 5mm; max-width: none; }
                    .no-print { display: none !important; }
                    @@page { size: A4; margin: 10mm; }
                }",
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            };

            await CreateTemplateAsync(defaultTemplate);
        }
    }
}
