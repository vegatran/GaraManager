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
        public string Status { get; set; } = "Draft"; // "Draft", "Sent", "Received", "Cancelled"
        
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
        
        /// <summary>
        /// ✅ 4.3.2.3: Số ngày credit được parse từ PaymentTerms (Net 30 → 30, COD → 0, Prepaid → -1)
        /// Dùng để tính DueDate và OverdueDays ở DB level
        /// </summary>
        public int? CreditDays { get; set; } // Số ngày credit (null = default 30)
        
        [StringLength(100)]
        public string? DeliveryTerms { get; set; } // Điều khoản giao hàng
        
        [StringLength(50)]
        public string? Currency { get; set; } = "VND"; // Đơn vị tiền tệ
        
        public decimal SubTotal { get; set; } = 0; // Tổng tiền hàng
        
        public decimal VATRate { get; set; } = 0; // Tỷ lệ thuế VAT
        
        public decimal TaxAmount { get; set; } = 0; // Tiền thuế
        
        public decimal ShippingCost { get; set; } = 0; // Phí vận chuyển
        
        public decimal TotalAmount { get; set; } = 0; // Tổng cộng
        
        public int? EmployeeId { get; set; } // Nhân viên tạo đơn
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; } // Người phê duyệt
        
        public DateTime? ApprovedDate { get; set; } // Ngày phê duyệt
        
        public DateTime? SentDate { get; set; } // Ngày gửi PO cho supplier
        
        public DateTime? ReceivedDate { get; set; } // Ngày nhận hàng
        
        public DateTime? CancelledDate { get; set; } // Ngày hủy PO
        
        [StringLength(500)]
        public string? CancelReason { get; set; } // Lý do hủy PO
        
        [StringLength(100)]
        public string? CancelledBy { get; set; } // Người hủy PO
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú
        
        public bool IsApproved { get; set; } = false; // Đã được phê duyệt
        
        // ✅ Phase 4.2.3: PO Tracking fields
        [StringLength(100)]
        public string? TrackingNumber { get; set; } // Mã vận đơn
        
        [StringLength(100)]
        public string? ShippingMethod { get; set; } // Phương thức vận chuyển
        
        public DateTime? InTransitDate { get; set; } // Ngày supplier gửi hàng
        
        // Note: ExpectedDeliveryDate đã có sẵn ở trên, nhưng có thể cập nhật
        // public DateTime? EstimatedDeliveryDate { get; set; } // Đã có ở trên
        
        [StringLength(20)]
        public string? DeliveryStatus { get; set; } // "OnTime", "Delayed", "AtRisk"
        
        [StringLength(500)]
        public string? DeliveryNotes { get; set; } // Ghi chú về giao hàng
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<PurchaseOrderStatusHistory> StatusHistory { get; set; } = new List<PurchaseOrderStatusHistory>();
    }
}
