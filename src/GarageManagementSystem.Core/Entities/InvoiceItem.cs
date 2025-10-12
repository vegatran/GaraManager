using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// InvoiceItem - Chi tiết hóa đơn
    /// </summary>
    public class InvoiceItem : BaseEntity
    {
        public int InvoiceId { get; set; }
        
        [Required]
        public int LineNumber { get; set; } // Số thứ tự dòng
        
        [Required]
        [StringLength(20)]
        public string ItemType { get; set; } = string.Empty; // Part, Service, Labor, Material
        
        [Required]
        [StringLength(500)]
        public string ItemName { get; set; } = string.Empty; // Tên hàng hóa/dịch vụ
        
        [StringLength(500)]
        public string? PartName { get; set; } // For parts
        
        [StringLength(500)]
        public string? ServiceName { get; set; } // For services
        
        [StringLength(100)]
        public string? ItemCode { get; set; } // Mã hàng hóa
        
        [StringLength(1000)]
        public string? Description { get; set; } // Mô tả chi tiết
        
        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty; // Đơn vị tính
        
        [Required]
        public decimal Quantity { get; set; } = 1; // Số lượng
        
        [Required]
        public decimal UnitPrice { get; set; } // Đơn giá
        
        [Required]
        public decimal TotalPrice { get; set; } // Thành tiền
        
        public decimal SubTotal { get; set; } // Subtotal before tax
        
        // Thuế
        public decimal TaxRate { get; set; } = 10; // Thuế suất (%)
        
        public decimal VATRate { get; set; } = 10; // Alias for TaxRate
        
        public decimal TaxAmount { get; set; } // Tiền thuế
        
        public decimal VATAmount { get; set; } // Alias for TaxAmount
        
        public decimal AmountIncludingTax { get; set; } // Tổng bao gồm thuế
        
        public decimal TotalAmount { get; set; } // Alias for AmountIncludingTax
        
        // Liên kết nguồn
        public int? PartId { get; set; }
        
        public int? ServiceId { get; set; }
        
        public int? LaborItemId { get; set; }
        
        public int? ServiceOrderItemId { get; set; }
        
        public int? ServiceOrderPartId { get; set; }
        
        public int? ServiceOrderLaborId { get; set; }
        
        // Thông tin hóa đơn đầu vào (để đối chiếu)
        [StringLength(50)]
        public string? InputInvoiceNumber { get; set; } // HĐ đầu vào (nếu là PT/VL)
        
        public DateTime? InputInvoiceDate { get; set; }
        
        public bool HasInputInvoice { get; set; } = false; // Có HĐ đầu vào
        
        // Phân loại
        [StringLength(50)]
        public string? Category { get; set; } // Thay thế, Sửa chữa, Sơn
        
        [StringLength(50)]
        public string? SubCategory { get; set; }
        
        // Ghi chú
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public int? DisplayOrder { get; set; } // Thứ tự hiển thị
        
        // Navigation properties
        public virtual Invoice Invoice { get; set; } = null!;
        public virtual Part? Part { get; set; }
        public virtual Service? Service { get; set; }
        public virtual LaborItem? LaborItem { get; set; }
        public virtual ServiceOrderItem? ServiceOrderItem { get; set; }
        public virtual ServiceOrderPart? ServiceOrderPart { get; set; }
        public virtual ServiceOrderLabor? ServiceOrderLabor { get; set; }
    }
}
