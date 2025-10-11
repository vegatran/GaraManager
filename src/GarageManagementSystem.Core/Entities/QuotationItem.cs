using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Quotation Item - Chi tiết báo giá
    /// </summary>
    public class QuotationItem : BaseEntity
    {
        public int ServiceQuotationId { get; set; }
        public int? ServiceId { get; set; }
        public int? PartId { get; set; } // Nếu là phụ tùng
        public int? InspectionIssueId { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

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

