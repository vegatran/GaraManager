using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PurchaseOrder - Đơn đặt hàng phụ tùng
    /// </summary>
    public class PurchaseOrder : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty; // "PO-2024-001"
        
        public int SupplierId { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now; // Ngày đặt hàng
        
        public DateTime? ExpectedDeliveryDate { get; set; } // Ngày giao hàng dự kiến
        
        public DateTime? ActualDeliveryDate { get; set; } // Ngày giao hàng thực tế
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Ordered", "PartiallyReceived", "Received", "Cancelled"
        
        [StringLength(50)]
        public string? SupplierOrderNumber { get; set; } // Số đơn hàng của nhà cung cấp
        
        [StringLength(100)]
        public string? ContactPerson { get; set; } // Người liên hệ
        
        [StringLength(20)]
        public string? ContactPhone { get; set; } // Số điện thoại liên hệ
        
        [StringLength(100)]
        public string? ContactEmail { get; set; } // Email liên hệ
        
        [StringLength(500)]
        public string? DeliveryAddress { get; set; } // Địa chỉ giao hàng
        
        [StringLength(50)]
        public string? PaymentTerms { get; set; } // Điều khoản thanh toán
        
        [StringLength(100)]
        public string? DeliveryTerms { get; set; } // Điều khoản giao hàng
        
        [StringLength(50)]
        public string? Currency { get; set; } = "VND"; // Đơn vị tiền tệ
        
        public decimal SubTotal { get; set; } = 0; // Tổng tiền hàng
        
        public decimal TaxAmount { get; set; } = 0; // Tiền thuế
        
        public decimal ShippingCost { get; set; } = 0; // Phí vận chuyển
        
        public decimal TotalAmount { get; set; } = 0; // Tổng cộng
        
        public int? EmployeeId { get; set; } // Nhân viên tạo đơn
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; } // Người phê duyệt
        
        public DateTime? ApprovedDate { get; set; } // Ngày phê duyệt
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú
        
        public bool IsApproved { get; set; } = false; // Đã được phê duyệt
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    }
}
