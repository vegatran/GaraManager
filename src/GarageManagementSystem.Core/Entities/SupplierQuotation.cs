using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// SupplierQuotation - Báo giá từ nhà cung cấp
    /// Phase 4.2.2: Đánh giá Nhà cung cấp
    /// </summary>
    public class SupplierQuotation : BaseEntity
    {
        public int SupplierId { get; set; }
        
        public int PartId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string QuotationNumber { get; set; } = string.Empty;
        
        [Required]
        public DateTime QuotationDate { get; set; } = DateTime.Now;
        
        public DateTime? ValidUntil { get; set; } // Ngày hết hạn báo giá
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public int MinimumOrderQuantity { get; set; } = 1;
        
        public int? LeadTimeDays { get; set; } // Thời gian giao hàng (ngày)
        
        [StringLength(100)]
        public string? WarrantyPeriod { get; set; } // Thời gian bảo hành
        
        [StringLength(500)]
        public string? WarrantyTerms { get; set; } // Điều khoản bảo hành
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Accepted", "Rejected", "Expired"
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;
    }
}

