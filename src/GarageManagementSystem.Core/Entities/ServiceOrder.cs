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
        
        /// <summary>
        /// ✅ 2.3.3: Liên kết đến ServiceOrder gốc (nếu là LSC Bổ sung từ phát sinh)
        /// </summary>
        public int? ParentServiceOrderId { get; set; }

        /// <summary>
        /// ✅ 2.3.3: Phân biệt JO gốc vs LSC Bổ sung
        /// </summary>
        public bool IsAdditionalOrder { get; set; } = false;
        
        [StringLength(1000)]
        public string? Description { get; set; } // Description of work
        
        public decimal EstimatedAmount { get; set; } = 0; // Estimated cost
        public decimal ActualAmount { get; set; } = 0; // Actual cost
        
        public DateTime? StartDate { get; set; } // When work started

        // ✅ 2.4: QC và Bàn giao
        /// <summary>
        /// ✅ 2.4.1: Tổng giờ công thực tế (tính từ tất cả items) - để tính lương
        /// </summary>
        public decimal? TotalActualHours { get; set; }

        /// <summary>
        /// ✅ 2.4.3: Số lần QC không đạt
        /// </summary>
        public int QCFailedCount { get; set; } = 0;

        /// <summary>
        /// ✅ 2.4.4: Ngày bàn giao xe
        /// </summary>
        public DateTime? HandoverDate { get; set; }

        /// <summary>
        /// ✅ 2.4.4: Khu vực bàn giao (ví dụ: "Khu vực tiếp đón", "Khu vực giao xe")
        /// </summary>
        [StringLength(200)]
        public string? HandoverLocation { get; set; }

        // ✅ 3.1: Quyết toán COGS (Cost of Goods Sold)
        /// <summary>
        /// ✅ 3.1: Tổng giá vốn hàng bán (COGS) - Tính từ vật tư đã xuất kho
        /// </summary>
        public decimal TotalCOGS { get; set; } = 0;

        /// <summary>
        /// ✅ 3.1: Phương pháp tính COGS ('FIFO' hoặc 'WeightedAverage')
        /// </summary>
        [StringLength(20)]
        public string COGSCalculationMethod { get; set; } = "FIFO"; // 'FIFO', 'WeightedAverage'

        /// <summary>
        /// ✅ 3.1: Ngày tính toán COGS lần cuối
        /// </summary>
        public DateTime? COGSCalculationDate { get; set; }

        /// <summary>
        /// ✅ 3.1: Chi tiết tính toán COGS (JSON) - Lưu breakdown theo từng vật tư
        /// </summary>
        [StringLength(5000)]
        public string? COGSBreakdown { get; set; }

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
        
        /// <summary>
        /// ✅ 2.3.3: Navigation property đến ServiceOrder gốc (nếu là LSC Bổ sung)
        /// </summary>
        public virtual ServiceOrder? ParentServiceOrder { get; set; }
        
        /// <summary>
        /// ✅ 2.3.3: Navigation property đến các LSC Bổ sung (nếu có)
        /// </summary>
        public virtual ICollection<ServiceOrder> AdditionalServiceOrders { get; set; } = new List<ServiceOrder>();
        
        public virtual ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
        public virtual ICollection<ServiceOrderPart> ServiceOrderParts { get; set; } = new List<ServiceOrderPart>();
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        
        /// <summary>
        /// ✅ 2.4.2: Navigation property đến QualityControl
        /// </summary>
        public virtual ICollection<QualityControl> QualityControls { get; set; } = new List<QualityControl>();
    }
}
