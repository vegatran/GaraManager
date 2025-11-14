using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Chi tiết phiếu điều chỉnh tồn kho
    /// </summary>
    public class InventoryAdjustmentItem : BaseEntity
    {
        [Required]
        public int InventoryAdjustmentId { get; set; } // ID phiếu điều chỉnh

        [Required]
        public int PartId { get; set; } // ID phụ tùng

        public int? InventoryCheckItemId { get; set; } // ID item trong phiếu kiểm kê (nếu có)

        [Required]
        public int QuantityChange { get; set; } // Số lượng thay đổi (có thể âm)

        [Required]
        public int SystemQuantityBefore { get; set; } // Số lượng trước điều chỉnh

        [Required]
        public int SystemQuantityAfter { get; set; } // Số lượng sau điều chỉnh

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual InventoryAdjustment InventoryAdjustment { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;
        public virtual InventoryCheckItem? InventoryCheckItem { get; set; }
    }
}

