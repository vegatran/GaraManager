using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho Print Template
    /// </summary>
    public class PrintTemplateDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string TemplateType { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(4000)]
        public string? HeaderHtml { get; set; }
        
        [StringLength(2000)]
        public string? FooterHtml { get; set; }
        
        [StringLength(2000)]
        public string? CompanyInfo { get; set; }
        
        [StringLength(4000)]
        public string? CustomCss { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
        
        [StringLength(200)]
        public string? LogoFileName { get; set; }
        
        [StringLength(500)]
        public string? LogoPath { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// DTO để tạo Print Template mới
    /// </summary>
    public class CreatePrintTemplateDto
    {
        [Required(ErrorMessage = "Tên mẫu là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên mẫu không được vượt quá 200 ký tự")]
        public string TemplateName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Loại mẫu là bắt buộc")]
        [StringLength(50, ErrorMessage = "Loại mẫu không được vượt quá 50 ký tự")]
        public string TemplateType { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
        
        [StringLength(4000, ErrorMessage = "Header HTML không được vượt quá 4000 ký tự")]
        public string? HeaderHtml { get; set; }
        
        [StringLength(2000, ErrorMessage = "Footer HTML không được vượt quá 2000 ký tự")]
        public string? FooterHtml { get; set; }
        
        [StringLength(2000, ErrorMessage = "Thông tin công ty không được vượt quá 2000 ký tự")]
        public string? CompanyInfo { get; set; }
        
        [StringLength(4000, ErrorMessage = "CSS tùy chỉnh không được vượt quá 4000 ký tự")]
        public string? CustomCss { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
        
        [StringLength(200, ErrorMessage = "Tên file logo không được vượt quá 200 ký tự")]
        public string? LogoFileName { get; set; }
        
        [StringLength(500, ErrorMessage = "Đường dẫn logo không được vượt quá 500 ký tự")]
        public string? LogoPath { get; set; }
        
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO để cập nhật Print Template
    /// </summary>
    public class UpdatePrintTemplateDto
    {
        [Required(ErrorMessage = "Tên mẫu là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên mẫu không được vượt quá 200 ký tự")]
        public string TemplateName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Loại mẫu là bắt buộc")]
        [StringLength(50, ErrorMessage = "Loại mẫu không được vượt quá 50 ký tự")]
        public string TemplateType { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
        
        [StringLength(4000, ErrorMessage = "Header HTML không được vượt quá 4000 ký tự")]
        public string? HeaderHtml { get; set; }
        
        [StringLength(2000, ErrorMessage = "Footer HTML không được vượt quá 2000 ký tự")]
        public string? FooterHtml { get; set; }
        
        [StringLength(2000, ErrorMessage = "Thông tin công ty không được vượt quá 2000 ký tự")]
        public string? CompanyInfo { get; set; }
        
        [StringLength(4000, ErrorMessage = "CSS tùy chỉnh không được vượt quá 4000 ký tự")]
        public string? CustomCss { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
        
        [StringLength(200, ErrorMessage = "Tên file logo không được vượt quá 200 ký tự")]
        public string? LogoFileName { get; set; }
        
        [StringLength(500, ErrorMessage = "Đường dẫn logo không được vượt quá 500 ký tự")]
        public string? LogoPath { get; set; }
        
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO để đặt mẫu làm mặc định
    /// </summary>
    public class SetDefaultTemplateDto
    {
        [Required(ErrorMessage = "Loại mẫu là bắt buộc")]
        public string TemplateType { get; set; } = string.Empty;
    }
}
