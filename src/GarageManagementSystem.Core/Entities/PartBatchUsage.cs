using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PartBatchUsage - Theo dõi sử dụng lô hàng
    /// </summary>
    public class PartBatchUsage : BaseEntity
    {
        public int PartInventoryBatchId { get; set; }
        
        public int ServiceOrderId { get; set; }
        
        public int? ServiceOrderPartId { get; set; } // Liên kết với ServiceOrderPart
        
        [Required]
        public int QuantityUsed { get; set; } // Số lượng sử dụng
        
        [Required]
        public decimal UnitCost { get; set; } // Giá vốn tại thời điểm sử dụng
        
        [Required]
        public decimal UnitPrice { get; set; } // Giá bán
        
        [Required]
        public decimal TotalCost { get; set; } // Tổng giá vốn
        
        [Required]
        public decimal TotalPrice { get; set; } // Tổng giá bán
        
        [Required]
        public DateTime UsageDate { get; set; } = DateTime.Now;
        
        // Thông tin khách hàng
        [StringLength(100)]
        public string? CustomerName { get; set; }
        
        public int? CustomerId { get; set; }
        
        [StringLength(20)]
        public string? CustomerType { get; set; } // Individual, Company, Insurance
        
        // Thông tin xe
        [StringLength(20)]
        public string? VehiclePlate { get; set; }
        
        public int? VehicleId { get; set; }
        
        // Thông tin hóa đơn xuất
        public bool RequiresInvoice { get; set; } = false; // Có cần xuất hóa đơn
        
        [StringLength(50)]
        public string? OutgoingInvoiceNumber { get; set; } // Số hóa đơn xuất
        
        public DateTime? InvoiceDate { get; set; }
        
        // Ghi chú
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public int? EmployeeId { get; set; }
        
        // Navigation properties
        public virtual PartInventoryBatch PartInventoryBatch { get; set; } = null!;
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual ServiceOrderPart? ServiceOrderPart { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
