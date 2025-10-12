using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// StockTransaction - Giao dịch kho (Nhập/Xuất/Tồn)
    /// </summary>
    public class StockTransaction : BaseEntity
    {
        public int PartId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TransactionNumber { get; set; } = string.Empty; // "STK-2024-001"
        
        [Required]
        [StringLength(20)]
        public StockTransactionType TransactionType { get; set; } // "In", "Out", "Transfer", "Adjustment"
        
        [Required]
        public int Quantity { get; set; } // Số lượng
        
        [Required]
        public decimal UnitCost { get; set; } // Giá đơn vị
        
        public decimal UnitPrice { get; set; } // Giá bán đơn vị
        
        [Required]
        public decimal TotalCost { get; set; } // Tổng giá trị
        
        public decimal TotalAmount { get; set; } // Tổng tiền bán
        
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        
        [StringLength(100)]
        public string? SupplierName { get; set; } // Tên nhà cung cấp (nếu nhập)
        
        public int? SupplierId { get; set; } // Foreign key to Supplier
        
        [StringLength(50)]
        public string? InvoiceNumber { get; set; } // Số hóa đơn
        
        public bool HasInvoice { get; set; } = false; // Có hóa đơn hay không
        
        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // Số tham chiếu
        
        [StringLength(30)]
        public string? SourceType { get; set; } = "Purchased"; // Purchased, Used, Refurbished, Salvage
        
        [StringLength(100)]
        public string? SourceReference { get; set; } // Nguồn gốc: "Tháo từ xe 30A-12345", "Mua từ chợ"
        
        [StringLength(20)]
        public string? Condition { get; set; } = "New"; // New, Used, Refurbished, AsIs
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú
        
        // Thông tin liên quan
        [StringLength(100)]
        public string? RelatedEntity { get; set; } // "ServiceOrder", "PurchaseOrder", "Return"
        
        public int? RelatedEntityId { get; set; } // ID của entity liên quan
        
        public int? ServiceOrderId { get; set; } // Liên quan đến đơn hàng (nếu xuất cho sửa chữa)
        
        public int? EmployeeId { get; set; } // Nhân viên thực hiện
        
        public int? ProcessedById { get; set; } // Nhân viên xử lý giao dịch
        
        [StringLength(100)]
        public string? Location { get; set; } // Vị trí kho
        
        // Thông tin tồn kho sau giao dịch
        public int StockAfter { get; set; } // Tồn kho sau giao dịch
        public int QuantityBefore { get; set; } // Tồn kho trước giao dịch
        public int QuantityAfter { get; set; } // Alias for StockAfter (API compatibility)
        
        // Navigation properties
        public virtual Part Part { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual ServiceOrder? ServiceOrder { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual Employee? ProcessedBy { get; set; }
    }
}
