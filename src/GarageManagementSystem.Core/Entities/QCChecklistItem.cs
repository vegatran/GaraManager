using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ✅ 2.4.2: QC Checklist Item - Chi tiết checklist kiểm tra QC
    /// </summary>
    public class QCChecklistItem : BaseEntity
    {
        [Required]
        public int QualityControlId { get; set; }

        /// <summary>
        /// Tên checklist item (ví dụ: "Kiểm tra động cơ", "Kiểm tra phanh", ...)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ChecklistItemName { get; set; } = string.Empty;

        /// <summary>
        /// Đã kiểm tra chưa
        /// </summary>
        public bool IsChecked { get; set; } = false;

        /// <summary>
        /// Kết quả: "Pass" (Đạt), "Fail" (Không đạt), null (Chưa kiểm tra)
        /// </summary>
        [StringLength(20)]
        public string? Result { get; set; } // "Pass", "Fail", null

        /// <summary>
        /// Ghi chú cho checklist item này
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        public virtual QualityControl QualityControl { get; set; } = null!;
    }
}

