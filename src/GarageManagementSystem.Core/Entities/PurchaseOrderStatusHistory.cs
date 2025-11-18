using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PurchaseOrderStatusHistory - Lịch sử trạng thái đơn đặt hàng
    /// Phase 4.2.3: Theo dõi PO (In-Transit Tracking)
    /// </summary>
    public class PurchaseOrderStatusHistory : BaseEntity
    {
        public int PurchaseOrderId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // "Sent", "InTransit", "Received", "Delayed"
        
        [Required]
        public DateTime StatusDate { get; set; } = DateTime.Now;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public int? UpdatedByEmployeeId { get; set; }
        
        // Navigation properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Employee? UpdatedByEmployee { get; set; }
    }
}

