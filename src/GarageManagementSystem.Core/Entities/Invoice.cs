using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Invoice - Hóa đơn VAT
    /// </summary>
    public class Invoice : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty; // Số hóa đơn
        
        [Required]
        [StringLength(20)]
        public string InvoiceSymbol { get; set; } = string.Empty; // Ký hiệu: AA/24E
        
        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        
        public DateTime? DueDate { get; set; } // Ngày đến hạn thanh toán
        
        [Required]
        [StringLength(20)]
        public string InvoiceType { get; set; } = "VAT"; // VAT, Sales, Export
        
        // Người bán (Garage)
        [Required]
        [StringLength(200)]
        public string SellerName { get; set; } = string.Empty; // Tên garage
        
        [Required]
        [StringLength(50)]
        public string SellerTaxCode { get; set; } = string.Empty; // MST garage
        
        [StringLength(200)]
        public string? SellerAddress { get; set; }
        
        [StringLength(20)]
        public string? SellerPhone { get; set; }
        
        // Người mua
        [Required]
        [StringLength(200)]
        public string BuyerName { get; set; } = string.Empty; // Tên công ty BH hoặc công ty
        
        [StringLength(50)]
        public string? BuyerTaxCode { get; set; } // MST người mua
        
        [StringLength(50)]
        public string? CustomerTaxCode { get; set; } // Alias for BuyerTaxCode
        
        [StringLength(200)]
        public string? BuyerAddress { get; set; }
        
        [StringLength(200)]
        public string? CustomerAddress { get; set; } // Alias for BuyerAddress
        
        [StringLength(20)]
        public string? BuyerPhone { get; set; }
        
        [StringLength(100)]
        public string? BuyerEmail { get; set; }
        
        // Liên kết đơn hàng
        public int? ServiceOrderId { get; set; }
        
        [StringLength(50)]
        public string? ServiceOrderNumber { get; set; }
        
        // Liên kết claim (nếu là xe bảo hiểm)
        public int? InsuranceClaimId { get; set; }
        
        [StringLength(50)]
        public string? ClaimNumber { get; set; }
        
        [StringLength(100)]
        public string? InsuranceCompany { get; set; } // Insurance company name
        
        // Thông tin xe (tham chiếu)
        public int? VehicleId { get; set; }
        
        [StringLength(20)]
        public string? VehiclePlate { get; set; }
        
        [StringLength(100)]
        public string? VehicleInfo { get; set; }
        
        [StringLength(50)]
        public string? VehicleMake { get; set; } // Hãng xe
        
        [StringLength(50)]
        public string? VehicleModel { get; set; } // Dòng xe
        
        public int? VehicleYear { get; set; } // Năm sản xuất // "Mercedes C-Class 2020"
        
        // Thông tin khách hàng cuối (chủ xe)
        public int? CustomerId { get; set; }
        
        [StringLength(200)]
        public string? CustomerName { get; set; }
        
        [StringLength(20)]
        public string? CustomerPhone { get; set; }
        
        // Tài chính
        [Required]
        public decimal SubTotal { get; set; } // Tổng trước thuế
        
        [Required]
        public decimal VATRate { get; set; } = 10; // Thuế suất VAT (%)
        
        [Required]
        public decimal VATAmount { get; set; } // Tiền thuế VAT
        
        [Required]
        public decimal TotalAmount { get; set; } // Tổng sau thuế
        
        public decimal DiscountAmount { get; set; } = 0; // Giảm giá
        
        public decimal FinalAmount { get; set; } // Thành tiền cuối
        
        [StringLength(3)]
        public string Currency { get; set; } = "VND";
        
        [StringLength(500)]
        public string? AmountInWords { get; set; } // Số tiền bằng chữ
        
        // Phương thức thanh toán
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Bank Transfer, Credit Card
        
        [StringLength(100)]
        public string? BankAccount { get; set; }
        
        [StringLength(100)]
        public string? BankName { get; set; }
        
        // Trạng thái
        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Issued, Cancelled, Adjusted
        
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Partial, Paid
        
        public decimal PaidAmount { get; set; } = 0; // Số tiền đã thanh toán
        
        public decimal RemainingAmount { get; set; } = 0; // Số tiền còn lại
        
        public DateTime? IssuedDate { get; set; } // Ngày phát hành
        
        public DateTime? PaidDate { get; set; } // Payment date
        public DateTime? PaymentDate { get; set; } // Alias for PaidDate
        
        public DateTime? CancelledDate { get; set; }
        
        [StringLength(500)]
        public string? CancellationReason { get; set; }
        
        // Hóa đơn điều chỉnh/thay thế
        public int? ReplacesInvoiceId { get; set; } // Thay thế hóa đơn nào
        
        public int? AdjustmentForInvoiceId { get; set; } // Điều chỉnh cho hóa đơn nào
        
        // Ghi chú
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; } // Mô tả nội dung
        
        // Người tạo
        public int? EmployeeId { get; set; }
        
        [StringLength(100)]
        public string? EmployeeName { get; set; }
        
        // Xác nhận
        public bool IsApproved { get; set; } = false;
        
        public DateTime? ApprovedDate { get; set; }
        
        [StringLength(100)]
        public string? ApprovedBy { get; set; }
        
        // Ký số
        public bool IsDigitallySigned { get; set; } = false;
        
        [StringLength(500)]
        public string? DigitalSignature { get; set; }
        
        public DateTime? SignedDate { get; set; }
        
        // Navigation properties
        public virtual ServiceOrder? ServiceOrder { get; set; }
        public virtual InsuranceClaim? InsuranceClaim { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual Invoice? ReplacesInvoice { get; set; }
        public virtual Invoice? AdjustmentForInvoice { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public virtual ICollection<Invoice> ReplacedByInvoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Invoice> AdjustmentInvoices { get; set; } = new List<Invoice>();
    }
}
