using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Quotation Item - Chi tiết báo giá
    /// </summary>
    public class QuotationItem : BaseEntity
    {
        public int ServiceQuotationId { get; set; }
        public int QuotationId { get; set; } // Alias for ServiceQuotationId (API compatibility)
        public int? ServiceId { get; set; }
        public int? PartId { get; set; } // Nếu là phụ tùng
        public int? InspectionIssueId { get; set; }
        
        [StringLength(50)]
        public string ItemType { get; set; } = "Service"; // Part or Service

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? PartName { get; set; } // For Part items
        
        [StringLength(200)]
        public string? ServiceName { get; set; } // For Service items

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal SubTotal { get; set; } // Quantity × UnitPrice
        
        public decimal DiscountAmount { get; set; } = 0; // Discount in amount
        
        public decimal DiscountPercent { get; set; } = 0; // Discount in percentage
        
        public decimal VATRate { get; set; } = 0.10m; // VAT rate
        
        public decimal VATAmount { get; set; } // SubTotal × VATRate
        
        [Required]
        public decimal TotalPrice { get; set; } // For backward compatibility
        
        public decimal TotalAmount { get; set; } // SubTotal + VATAmount - Discount
        
        public decimal FinalPrice { get; set; } = 0; // Final price after discount

        public bool IsOptional { get; set; } = false;

        public bool IsApproved { get; set; } = false;

        [StringLength(500)]
        public string? Notes { get; set; }

        public int? DisplayOrder { get; set; }

        // Navigation properties
        public virtual ServiceQuotation ServiceQuotation { get; set; } = null!;
        public virtual Service? Service { get; set; }
        public virtual Part? Part { get; set; }
        public virtual InspectionIssue? InspectionIssue { get; set; }
    }
}

