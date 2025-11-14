using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Phiếu điều chỉnh tồn kho (từ kiểm kê hoặc thủ công)
    /// </summary>
    public class InventoryAdjustment : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string AdjustmentNumber { get; set; } = string.Empty; // Mã phiếu điều chỉnh (VD: ADJ-2024-001)

        public int? InventoryCheckId { get; set; } // ID phiếu kiểm kê (nếu có)

        public int? WarehouseId { get; set; }
        public int? WarehouseZoneId { get; set; }
        public int? WarehouseBinId { get; set; }

        [Required]
        public DateTime AdjustmentDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(1000)]
        public string? Reason { get; set; } // Lý do điều chỉnh

        public int? ApprovedByEmployeeId { get; set; } // ID nhân viên duyệt
        public DateTime? ApprovedAt { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; } // Lý do từ chối

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual InventoryCheck? InventoryCheck { get; set; }
        public virtual Warehouse? Warehouse { get; set; }
        public virtual WarehouseZone? WarehouseZone { get; set; }
        public virtual WarehouseBin? WarehouseBin { get; set; }
        public virtual Employee? ApprovedByEmployee { get; set; }
        public virtual ICollection<InventoryAdjustmentItem> Items { get; set; } = new List<InventoryAdjustmentItem>();
    }
}

