using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceQuotationDto : BaseDto
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public int? VehicleInspectionId { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int? PreparedById { get; set; }
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? Description { get; set; }
        public string? Terms { get; set; }
        
        // Quotation Type
        public string QuotationType { get; set; } = "Personal";
        
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal VATAmount { get; set; } // ✅ THÊM VATAmount
        public decimal TaxRate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Insurance specific fields (nullable)
        public decimal? MaxInsuranceAmount { get; set; }
        public decimal? Deductible { get; set; }
        public DateTime? InsuranceApprovalDate { get; set; }
        public decimal? InsuranceApprovedAmount { get; set; }
        public string? InsuranceApprovalNotes { get; set; }
        public string? InsuranceAdjusterContact { get; set; }

        // Company specific fields (nullable)
        public string? PONumber { get; set; }
        public string? PaymentTerms { get; set; } = "Cash";
        public bool IsTaxExempt { get; set; } = false;
        public DateTime? CompanyApprovalDate { get; set; }
        public string? CompanyApprovedBy { get; set; }
        public string? CompanyApprovalNotes { get; set; }
        public string? CompanyContactPerson { get; set; }
        
        // Insurance pricing - Giá bảo hiểm duyệt
        public decimal? InsuranceApprovedSubTotal { get; set; }
        public decimal? InsuranceApprovedTaxAmount { get; set; }
        public decimal? InsuranceApprovedDiscountAmount { get; set; }
        public decimal? InsuranceApprovedTotalAmount { get; set; }
        
        public string Status { get; set; } = string.Empty;
        public DateTime? SentDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? CustomerNotes { get; set; }
        public string? RejectionReason { get; set; }
        public int? ServiceOrderId { get; set; }
        
        /// <summary>
        /// ✅ 2.3.3: Liên kết đến ServiceOrder gốc (nếu là báo giá bổ sung từ phát sinh)
        /// </summary>
        public int? RelatedToServiceOrderId { get; set; }

        /// <summary>
        /// ✅ 2.3.3: Phân biệt báo giá gốc vs báo giá bổ sung từ phát sinh
        /// </summary>
        public bool IsAdditionalQuotation { get; set; } = false;

        // Navigation properties
        public VehicleInspectionDto? VehicleInspection { get; set; }
        public CustomerDto? Customer { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public EmployeeDto? PreparedBy { get; set; }
        public List<QuotationItemDto> Items { get; set; } = new();
        public List<QuotationAttachmentDto> Attachments { get; set; } = new();
    }

    public class CreateServiceQuotationDto
    {
        public int? VehicleInspectionId { get; set; }
        public int? CustomerReceptionId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        public int? PreparedById { get; set; }

        public DateTime? ValidUntil { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? Terms { get; set; }

        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;
        
        /// <summary>
        /// ✅ 2.3.3: Liên kết đến ServiceOrder gốc (nếu là báo giá bổ sung từ phát sinh)
        /// </summary>
        public int? RelatedToServiceOrderId { get; set; }

        /// <summary>
        /// ✅ 2.3.3: Phân biệt báo giá gốc vs báo giá bổ sung từ phát sinh
        /// </summary>
        public bool IsAdditionalQuotation { get; set; } = false;

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateQuotationItemDto> Items { get; set; } = new();
    }

    public class UpdateServiceQuotationDto
    {
        [Required]
        public int Id { get; set; }

        public DateTime? ValidUntil { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? Terms { get; set; }

        [Range(0, 100)]
        public decimal TaxRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        [StringLength(20)]
        public string? QuotationType { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(5000)]
        public string? CustomerNotes { get; set; }

        [StringLength(5000)]
        public string? RejectionReason { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateQuotationItemDto> Items { get; set; } = new();
    }


    public class QuotationItemDto
    {
        public int Id { get; set; }
        public int ServiceQuotationId { get; set; }
        public int? ServiceId { get; set; }
        public int? PartId { get; set; } // ✅ THÊM: ID của phụ tùng (nếu item là phụ tùng)
        public int? InspectionIssueId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsOptional { get; set; }
        public bool IsApproved { get; set; }
        public bool HasInvoice { get; set; }
        public bool IsVATApplicable { get; set; }
        public decimal? VATRate { get; set; } // ✅ THÊM VATRate
        
        // ✅ THÊM: VAT Override từ Part (READ-ONLY - KHÔNG ĐƯỢC CHỈNH SỬA)
        public decimal? OverrideVATRate { get; set; } // Ghi đè VAT từ Part (CHỈ ĐỌC)
        public bool? OverrideIsVATApplicable { get; set; } // Ghi đè áp dụng VAT từ Part (CHỈ ĐỌC)
        
        // ✅ THÊM: Thông tin VAT từ Part (READ-ONLY)
        public decimal? PartVATRate { get; set; } // VAT rate từ Part entity
        public bool? PartIsVATApplicable { get; set; } // Có áp dụng VAT từ Part entity
        
        // Insurance approved pricing - Giá bảo hiểm duyệt
        public decimal? InsuranceApprovedUnitPrice { get; set; }
        public decimal? InsuranceApprovedSubTotal { get; set; }
        public decimal? InsuranceApprovedVATAmount { get; set; }
        public decimal? InsuranceApprovedTotalAmount { get; set; }
        public string? InsuranceApprovalNotes { get; set; }
        
        public string? Notes { get; set; }
        public string? ServiceType { get; set; } // parts, repair, paint
        public string? ItemCategory { get; set; } = "Material"; // Material, Service, Labor
        public ServiceDto? Service { get; set; }
        public InspectionIssueDto? InspectionIssue { get; set; }
    }

    public class CreateQuotationItemDto
    {
        public int? ServiceId { get; set; }
        public int? InspectionIssueId { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public bool IsOptional { get; set; } = false;

        public bool HasInvoice { get; set; } = false;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? ServiceType { get; set; } // parts, repair, paint
        
        [StringLength(50)]
        public string? ItemCategory { get; set; } = "Material"; // Material, Service, Labor

        public bool IsVATApplicable { get; set; } = false; // ✅ THÊM: VAT áp dụng
        
        [Range(0, 100)]
        public decimal VATRate { get; set; } = 10; // ✅ THÊM: VAT rate
        
        // ❌ XÓA: Không cho phép Override VAT trong CreateQuotationItemDto
        // VAT sẽ được lấy tự động từ Part entity (READ-ONLY)
    }

    public class QuotationAttachmentDto
    {
        public int Id { get; set; }
        public int ServiceQuotationId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public string AttachmentType { get; set; } = "General";
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public bool IsInsuranceDocument { get; set; } = false;
        public DateTime UploadedDate { get; set; }
        public int? UploadedById { get; set; }
        public EmployeeDto? UploadedBy { get; set; }
    }

    public class CreateQuotationAttachmentDto
    {
        public int ServiceQuotationId { get; set; }
        public string AttachmentType { get; set; } = "General";
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public bool IsInsuranceDocument { get; set; } = false;
    }

    /// <summary>
    /// DTO cho việc cập nhật bảng giá duyệt của bảo hiểm
    /// </summary>
    public class InsuranceApprovedPricingDto
    {
        public int QuotationId { get; set; }
        public string InsuranceCompany { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty; // ✅ THÊM: Mã số thuế
        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal CustomerCoPayment { get; set; }
        public string? ApprovalNotes { get; set; }
        public string? InsuranceFilePath { get; set; } // ✅ THÊM: Đường dẫn file bảo hiểm
        public List<InsuranceApprovedItemDto> ApprovedItems { get; set; } = new List<InsuranceApprovedItemDto>();
    }

    /// <summary>
    /// DTO cho từng item được duyệt bởi bảo hiểm
    /// </summary>
    public class InsuranceApprovedItemDto
    {
        public int QuotationItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal ApprovedPrice { get; set; }
        public decimal CustomerCoPayment { get; set; }
        public bool IsApproved { get; set; } = true;
        public string? ApprovalNotes { get; set; }
    }

    public class CorporateApprovedPricingDto
    {
        public int QuotationId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal CustomerCoPayment { get; set; }
        public string ApprovalNotes { get; set; } = string.Empty;
        public List<CorporateApprovedItemDto> ApprovedItems { get; set; } = new List<CorporateApprovedItemDto>();
    }

    public class CorporateApprovedItemDto
    {
        public int QuotationItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal ApprovedPrice { get; set; }
        public decimal CustomerCoPayment { get; set; }
        public bool IsApproved { get; set; } = true;
        public string? ApprovalNotes { get; set; }
    }

    public class UpdateQuotationStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}

