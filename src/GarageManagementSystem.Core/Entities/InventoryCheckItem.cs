using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Chi tiết phiếu kiểm kê
    /// </summary>
    public class InventoryCheckItem : BaseEntity
    {
        [Required]
        public int InventoryCheckId { get; set; } // ID phiếu kiểm kê

        [Required]
        public int PartId { get; set; } // ID phụ tùng

        [Required]
        public int SystemQuantity { get; set; } // Số lượng theo hệ thống

        [Required]
        public int ActualQuantity { get; set; } // Số lượng thực tế

        public int DiscrepancyQuantity { get; set; } // Chênh lệch (ActualQuantity - SystemQuantity)

        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú về chênh lệch

        public bool IsDiscrepancy { get; set; } = false; // Có chênh lệch không

        public bool IsAdjusted { get; set; } = false; // Đã điều chỉnh chưa

        public int? InventoryAdjustmentItemId { get; set; } // ID item trong phiếu điều chỉnh (nếu có)

        // Navigation properties
        public virtual InventoryCheck InventoryCheck { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;
        public virtual InventoryAdjustmentItem? InventoryAdjustmentItem { get; set; }
    }
}

