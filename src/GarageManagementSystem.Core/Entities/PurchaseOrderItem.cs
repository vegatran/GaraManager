using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PurchaseOrderItem - Chi tiết đơn đặt hàng
    /// </summary>
    public class PurchaseOrderItem : BaseEntity
    {
        public int PurchaseOrderId { get; set; }
        
        public int PartId { get; set; }
        
        [StringLength(200)]
        public string? PartName { get; set; } // Cached part name
        
        [Required]
        public int QuantityOrdered { get; set; } // Số lượng đặt
        
        public int QuantityReceived { get; set; } = 0; // Số lượng đã nhận
        
        [Required]
        public decimal UnitPrice { get; set; } // Giá đơn vị
        
        [Required]
        public decimal TotalPrice { get; set; } // Tổng tiền = QuantityOrdered × UnitPrice
        
        [StringLength(50)]
        public string? SupplierPartNumber { get; set; } // Mã phụ tùng của nhà cung cấp
        
        [StringLength(100)]
        public string? PartDescription { get; set; } // Mô tả phụ tùng
        
        [StringLength(50)]
        public string? Unit { get; set; } // Đơn vị (cái, lít, bộ...)
        
        public DateTime? ExpectedDeliveryDate { get; set; } // Ngày giao hàng dự kiến cho item này
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú
        
        public bool IsReceived { get; set; } = false; // Đã nhận hàng
        
        public DateTime? ReceivedDate { get; set; } // Ngày nhận hàng
        
        // Navigation properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;
    }
}
