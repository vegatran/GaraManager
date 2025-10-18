using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Mẫu in báo giá, hóa đơn và các tài liệu khác
    /// </summary>
    public class PrintTemplate : BaseEntity
    {
        /// <summary>
        /// Tên mẫu in
        /// </summary>
        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; } = string.Empty;
        
        /// <summary>
        /// Loại mẫu: Quotation, Invoice, Order, Receipt
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TemplateType { get; set; } = string.Empty;
        
        /// <summary>
        /// Mô tả mẫu
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// HTML header của mẫu
        /// </summary>
        [StringLength(4000)]
        public string? HeaderHtml { get; set; }
        
        /// <summary>
        /// HTML footer của mẫu
        /// </summary>
        [StringLength(2000)]
        public string? FooterHtml { get; set; }
        
        /// <summary>
        /// Thông tin công ty (JSON format)
        /// </summary>
        [StringLength(2000)]
        public string? CompanyInfo { get; set; }
        
        /// <summary>
        /// CSS styles cho mẫu
        /// </summary>
        [StringLength(4000)]
        public string? CustomCss { get; set; }
        
        /// <summary>
        /// Có phải mẫu mặc định không
        /// </summary>
        public bool IsDefault { get; set; } = false;
        
        /// <summary>
        /// Mẫu có đang hoạt động không
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
        
        /// <summary>
        /// Tên file logo
        /// </summary>
        [StringLength(200)]
        public string? LogoFileName { get; set; }
        
        /// <summary>
        /// Đường dẫn logo
        /// </summary>
        [StringLength(500)]
        public string? LogoPath { get; set; }
        
        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
