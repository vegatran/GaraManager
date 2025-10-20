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
        
        [StringLength(50)]
        public string ItemCategory { get; set; } = "Material"; // Material, Service, Labor

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

        /// <summary>
        /// Có hóa đơn hay không (chỉ áp dụng cho phụ tùng)
        /// </summary>
        public bool HasInvoice { get; set; } = false;

        // Pricing Model Support
        [StringLength(20)]
        public string PricingModel { get; set; } = "Combined"; // "Combined", "Separated", "LaborOnly"
        
        public decimal MaterialCost { get; set; } = 0; // Chi phí vật liệu
        
        public decimal LaborCost { get; set; } = 0; // Chi phí công lao động
        
        public bool IsVATApplicable { get; set; } = true; // Có chịu VAT không
        
        // Insurance approved pricing - Giá bảo hiểm duyệt
        public decimal? InsuranceApprovedUnitPrice { get; set; }
        public decimal? InsuranceApprovedSubTotal { get; set; }
        public decimal? InsuranceApprovedVATAmount { get; set; }
        public decimal? InsuranceApprovedTotalAmount { get; set; }
        public string? InsuranceApprovalNotes { get; set; }

        // Corporate approved pricing - Giá công ty duyệt
        public decimal? CorporateApprovedUnitPrice { get; set; }
        public decimal? CorporateApprovedSubTotal { get; set; }
        public decimal? CorporateApprovedVATAmount { get; set; }
        public decimal? CorporateApprovedTotalAmount { get; set; }
        public string? CorporateApprovalNotes { get; set; }
        
        [StringLength(1500)]
        public string? PricingBreakdown { get; set; } // Chi tiết phân tích giá (JSON)

        [StringLength(1500)]
        public string? Notes { get; set; }

        public int? DisplayOrder { get; set; }

        // Navigation properties
        public virtual ServiceQuotation ServiceQuotation { get; set; } = null!;
        public virtual Service? Service { get; set; }
        public virtual Part? Part { get; set; }
        public virtual InspectionIssue? InspectionIssue { get; set; }
    }
}

