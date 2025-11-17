using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Phiếu kiểm kê định kỳ
    /// </summary>
    public class InventoryCheck : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty; // Mã phiếu kiểm kê (VD: IK-2024-001)

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty; // Tên phiếu kiểm kê

        [StringLength(500)]
        public string? Description { get; set; } // Mô tả

        public DateTime CheckDate { get; set; } // Ngày kiểm kê

        public int? WarehouseId { get; set; } // Kho kiểm kê (nullable - có thể kiểm kê toàn bộ kho)

        public int? WarehouseZoneId { get; set; } // Khu vực kiểm kê (nullable)

        public int? WarehouseBinId { get; set; } // Kệ/Ngăn kiểm kê (nullable)

        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Trạng thái: Draft, InProgress, Completed, Cancelled

        public DateTime? StartedDate { get; set; } // Ngày bắt đầu kiểm kê

        public DateTime? CompletedDate { get; set; } // Ngày hoàn thành kiểm kê

        public int? StartedByEmployeeId { get; set; } // Người bắt đầu kiểm kê

        public int? CompletedByEmployeeId { get; set; } // Người hoàn thành kiểm kê

        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú

        // Navigation properties
        public virtual Warehouse? Warehouse { get; set; }
        public virtual WarehouseZone? WarehouseZone { get; set; }
        public virtual WarehouseBin? WarehouseBin { get; set; }
        public virtual Employee? StartedByEmployee { get; set; }
        public virtual Employee? CompletedByEmployee { get; set; }
        public virtual ICollection<InventoryCheckItem> Items { get; set; } = new List<InventoryCheckItem>();
        // ✅ Phase 4.1 - Advanced Features: Comments/Notes Timeline
        public virtual ICollection<InventoryCheckComment> Comments { get; set; } = new List<InventoryCheckComment>();
    }
}

