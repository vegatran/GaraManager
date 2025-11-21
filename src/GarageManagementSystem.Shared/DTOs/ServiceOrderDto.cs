using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceOrderDto : BaseDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public CustomerDto? Customer { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public int? ServiceQuotationId { get; set; } // ✅ THÊM: ID của báo giá gốc
        
        /// <summary>
        /// ✅ 2.3.3: Liên kết đến ServiceOrder gốc (nếu là LSC Bổ sung từ phát sinh)
        /// </summary>
        public int? ParentServiceOrderId { get; set; }

        /// <summary>
        /// ✅ 2.3.3: Phân biệt JO gốc vs LSC Bổ sung
        /// </summary>
        public bool IsAdditionalOrder { get; set; } = false;
        
        /// <summary>
        /// ✅ 2.3.3: Tổng tiền của các LSC Bổ sung (chỉ có giá trị khi là JO gốc)
        /// </summary>
        public decimal AdditionalOrdersTotalAmount { get; set; } = 0;
        
        /// <summary>
        /// ✅ 2.3.3: Tổng tiền cuối cùng bao gồm JO gốc + LSC Bổ sung (chỉ có giá trị khi là JO gốc)
        /// </summary>
        public decimal GrandTotalAmount { get; set; } = 0;

        /// <summary>
        /// ✅ 2.4: QC và Bàn giao
        /// </summary>
        public decimal? TotalActualHours { get; set; }
        public int QCFailedCount { get; set; } = 0;
        public DateTime? HandoverDate { get; set; }
        public string? HandoverLocation { get; set; }
        public string? WarrantyCode { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// ✅ 3.1: Quyết toán COGS (Cost of Goods Sold)
        /// </summary>
        public decimal TotalCOGS { get; set; } = 0;
        public string COGSCalculationMethod { get; set; } = "FIFO"; // 'FIFO' hoặc 'WeightedAverage'
        public DateTime? COGSCalculationDate { get; set; }
        public string? COGSBreakdown { get; set; } // JSON breakdown

        public List<ServiceOrderItemDto> ServiceOrderItems { get; set; } = new();
        public List<WarrantyDto> Warranties { get; set; } = new();
        public List<ServiceOrderFeeDto> ServiceOrderFees { get; set; } = new();
        public List<ServiceOrderPartDto> ServiceOrderParts { get; set; } = new();
    }

    public class CreateServiceOrderDto
    {
        [Required(ErrorMessage = "Khách hàng là bắt buộc")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Xe là bắt buộc")]
        public int VehicleId { get; set; }

        public int? ServiceQuotationId { get; set; }
        public int? CustomerReceptionId { get; set; }
        
        /// <summary>
        /// ✅ 2.3.3: Liên kết đến ServiceOrder gốc (nếu là LSC Bổ sung từ phát sinh)
        /// </summary>
        public int? ParentServiceOrderId { get; set; }

        /// <summary>
        /// ✅ 2.3.3: Phân biệt JO gốc vs LSC Bổ sung
        /// </summary>
        public bool IsAdditionalOrder { get; set; } = false;

        public DateTime? ScheduledDate { get; set; }

        [StringLength(5000, ErrorMessage = "Ghi chú không được vượt quá 5000 ký tự")]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá phải lớn hơn hoặc bằng 0")]
        public decimal DiscountAmount { get; set; } = 0;

        public List<CreateServiceOrderItemDto> ServiceOrderItems { get; set; } = new();
    }

    public class UpdateServiceOrderDto
    {
        [Required]
        public int Id { get; set; }

        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string? Status { get; set; }

        [StringLength(5000, ErrorMessage = "Ghi chú không được vượt quá 5000 ký tự")]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá phải lớn hơn hoặc bằng 0")]
        public decimal DiscountAmount { get; set; }

        [StringLength(50, ErrorMessage = "Trạng thái thanh toán không được vượt quá 50 ký tự")]
        public string? PaymentStatus { get; set; }
    }
}
