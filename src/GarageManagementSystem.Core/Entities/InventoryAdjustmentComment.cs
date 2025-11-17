using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ✅ Phase 4.1 - Advanced Features: Comment/Note cho Inventory Adjustment
    /// Lưu timeline các ghi chú và bình luận
    /// </summary>
    public class InventoryAdjustmentComment : BaseEntity
    {
        [Required]
        public int InventoryAdjustmentId { get; set; }

        [Required]
        [StringLength(2000)]
        public string CommentText { get; set; } = string.Empty;

        public int? CreatedByEmployeeId { get; set; } // ID nhân viên tạo comment

        [StringLength(100)]
        public string? CreatedByUserName { get; set; } // Tên user (backup nếu employee bị xóa)

        // Navigation properties
        public virtual InventoryAdjustment InventoryAdjustment { get; set; } = null!;
        public virtual Employee? CreatedByEmployee { get; set; }
    }
}

