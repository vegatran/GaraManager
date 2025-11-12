using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ✅ OPTIMIZED: QC Checklist Template - Template mẫu cho QC Checklist
    /// Cho phép admin quản lý các hạng mục kiểm tra QC mặc định
    /// </summary>
    public class QCChecklistTemplate : BaseEntity
    {
        /// <summary>
        /// Tên template (ví dụ: "Template Mặc Định", "Template Cho Xe Bảo Hiểm", ...)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả template
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Template mặc định (chỉ có 1 template mặc định)
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Loại xe áp dụng: null = Tất cả, "Personal", "Insurance", "Company"
        /// </summary>
        [StringLength(50)]
        public string? VehicleType { get; set; }

        /// <summary>
        /// Loại dịch vụ áp dụng: null = Tất cả, "Repair", "Paint", "General"
        /// </summary>
        [StringLength(50)]
        public string? ServiceType { get; set; }

        /// <summary>
        /// Template có đang active không
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<QCChecklistTemplateItem> TemplateItems { get; set; } = new List<QCChecklistTemplateItem>();
    }
}

