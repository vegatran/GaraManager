using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Service Quotation - Báo giá dịch vụ
    /// </summary>
    public class ServiceQuotation : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string QuotationNumber { get; set; } = string.Empty;

        public int? VehicleInspectionId { get; set; }
        public int? InspectionId { get; set; } // Alias for VehicleInspectionId
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int? PreparedById { get; set; } // Employee who prepared quotation

        [Required]
        public DateTime QuotationDate { get; set; } = DateTime.Now;

        public DateTime? ValidUntil { get; set; }
        public DateTime? ExpiryDate { get; set; } // Alias for ValidUntil

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? Terms { get; set; }

        // Quotation Type
        [StringLength(20)]
        public string QuotationType { get; set; } = "Personal"; // Personal, Insurance, Company

        // Pricing
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; } = 0;
        public decimal VATAmount { get; set; } = 0; // Alias for TaxAmount
        public decimal TaxRate { get; set; } = 0; // Tax percentage
        public decimal DiscountAmount { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0; // Discount percentage
        public decimal TotalAmount { get; set; }

        // Insurance specific fields (nullable)
        public decimal? MaxInsuranceAmount { get; set; }
        public decimal? Deductible { get; set; }
        public DateTime? InsuranceApprovalDate { get; set; }
        public decimal? InsuranceApprovedAmount { get; set; }
        public string? InsuranceApprovalNotes { get; set; }
        public string? InsuranceAdjusterContact { get; set; }
        
        // Insurance pricing - Giá bảo hiểm duyệt
        public decimal? InsuranceApprovedSubTotal { get; set; }
        public decimal? InsuranceApprovedTaxAmount { get; set; }
        public decimal? InsuranceApprovedDiscountAmount { get; set; }
        public decimal? InsuranceApprovedTotalAmount { get; set; }

        // Company specific fields (nullable)
        public string? PONumber { get; set; }
        public string? PaymentTerms { get; set; } = "Cash"; // Cash, Net15, Net30, Net45
        public bool IsTaxExempt { get; set; } = false;
        public DateTime? CompanyApprovalDate { get; set; }
        public string? CompanyApprovedBy { get; set; }
        public string? CompanyApprovalNotes { get; set; }
        public string? CompanyContactPerson { get; set; }

        // Status
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Approved, Rejected, Expired, Cancelled

        public DateTime? SentDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }

        [StringLength(1000)]
        public string? CustomerNotes { get; set; }

        [StringLength(1000)]
        public string? RejectionReason { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; } // Alias for CustomerNotes
        
        // Cached customer/vehicle info for API
        [StringLength(200)]
        public string? CustomerName { get; set; }
        
        [StringLength(20)]
        public string? CustomerPhone { get; set; }
        
        [StringLength(100)]
        public string? CustomerEmail { get; set; }
        
        [StringLength(20)]
        public string? VehiclePlate { get; set; }
        
        [StringLength(50)]
        public string? VehicleMake { get; set; }
        
        [StringLength(50)]
        public string? VehicleModel { get; set; }

        // If approved, reference to created ServiceOrder
        public int? ServiceOrderId { get; set; }

        // Reference to Customer Reception (for workflow tracking)
        public int? CustomerReceptionId { get; set; }

        // Navigation properties
        public virtual VehicleInspection? VehicleInspection { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Employee? PreparedBy { get; set; }
        public virtual CustomerReception? CustomerReception { get; set; }
        public virtual ServiceOrder? ServiceOrder { get; set; }
        public virtual ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
        public virtual ICollection<QuotationAttachment> Attachments { get; set; } = new List<QuotationAttachment>();
    }
}

