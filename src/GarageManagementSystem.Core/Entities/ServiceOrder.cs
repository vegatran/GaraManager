using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class ServiceOrder : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public int VehicleId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled

        [StringLength(1000)]
        public string? Notes { get; set; }

        public decimal SubTotal { get; set; } = 0; // Tổng trước thuế
        public decimal VATAmount { get; set; } = 0; // Tiền thuế VAT
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }

        [StringLength(50)]
        public string? PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Partial

        // Reference to inspection and quotation
        public int? VehicleInspectionId { get; set; }
        public int? ServiceQuotationId { get; set; }
        public int? QuotationId { get; set; } // Alias for ServiceQuotationId (API compatibility)
        public int? InsuranceClaimId { get; set; } // Link to insurance claim
        
        // Reference to Customer Reception (for workflow tracking)
        public int? CustomerReceptionId { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; } // Description of work
        
        public decimal EstimatedAmount { get; set; } = 0; // Estimated cost
        public decimal ActualAmount { get; set; } = 0; // Actual cost
        
        public DateTime? StartDate { get; set; } // When work started

        // Assigned employees
        public int? PrimaryTechnicianId { get; set; }

        // Payment tracking (calculated fields)
        public decimal ServiceTotal { get; set; } = 0;  // Tổng tiền dịch vụ
        public decimal PartsTotal { get; set; } = 0;    // Tổng tiền phụ tùng
        public decimal AmountPaid { get; set; } = 0;    // Đã thanh toán
        public decimal AmountRemaining { get; set; } = 0; // Còn nợ

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual VehicleInspection? VehicleInspection { get; set; }
        public virtual ServiceQuotation? ServiceQuotation { get; set; }
        public virtual CustomerReception? CustomerReception { get; set; }
        public virtual Employee? PrimaryTechnician { get; set; }
        public virtual ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
        public virtual ICollection<ServiceOrderPart> ServiceOrderParts { get; set; } = new List<ServiceOrderPart>();
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
