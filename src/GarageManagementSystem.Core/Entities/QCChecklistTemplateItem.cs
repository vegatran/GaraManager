using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ✅ OPTIMIZED: QC Checklist Template Item - Chi tiết hạng mục trong template
    /// </summary>
    public class QCChecklistTemplateItem : BaseEntity
    {
        [Required]
        public int TemplateId { get; set; }

        /// <summary>
        /// Tên hạng mục kiểm tra (ví dụ: "Kiểm tra chất lượng sơn", "Kiểm tra động cơ", ...)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ChecklistItemName { get; set; } = string.Empty;

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Hạng mục có bắt buộc không (nếu true, không thể xóa khi tạo QC)
        /// </summary>
        public bool IsRequired { get; set; } = false;

        // Navigation properties
        public virtual QCChecklistTemplate Template { get; set; } = null!;
    }
}

