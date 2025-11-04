using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// Additional Issue DTO - Phát sinh phát hiện trong quá trình sửa chữa
    /// </summary>
    public class AdditionalIssueDto : BaseDto
    {
        public int ServiceOrderId { get; set; }
        public int? ServiceOrderItemId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string IssueName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool RequiresImmediateAction { get; set; }
        public int ReportedByEmployeeId { get; set; }
        public string? TechnicianNotes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public int? AdditionalQuotationId { get; set; }
        public int? AdditionalServiceOrderId { get; set; }

        // Navigation properties
        public ServiceOrderDto? ServiceOrder { get; set; }
        public ServiceOrderItemDto? ServiceOrderItem { get; set; }
        public EmployeeDto? ReportedByEmployee { get; set; }
        public ServiceQuotationDto? AdditionalQuotation { get; set; }
        public List<AdditionalIssuePhotoDto> Photos { get; set; } = new();
    }

    /// <summary>
    /// Create Additional Issue DTO
    /// </summary>
    public class CreateAdditionalIssueDto
    {
        [Required(ErrorMessage = "Phiếu sửa chữa là bắt buộc")]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// ServiceOrderItem bị ảnh hưởng (nếu null, phát sinh ảnh hưởng toàn bộ ServiceOrder)
        /// </summary>
        public int? ServiceOrderItemId { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên phát sinh là bắt buộc")]
        [StringLength(200)]
        public string IssueName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low

        public bool RequiresImmediateAction { get; set; } = false;

        [StringLength(1000)]
        public string? TechnicianNotes { get; set; }

        /// <summary>
        /// Danh sách đường dẫn file ảnh (sẽ được xử lý bởi Controller)
        /// </summary>
        public List<string>? PhotoFilePaths { get; set; }
    }

    /// <summary>
    /// Update Additional Issue DTO
    /// </summary>
    public class UpdateAdditionalIssueDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(200)]
        public string? IssueName { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Severity { get; set; }

        public bool? RequiresImmediateAction { get; set; }

        [StringLength(1000)]
        public string? TechnicianNotes { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        /// <summary>
        /// Danh sách đường dẫn file ảnh mới (sẽ được xử lý bởi Controller)
        /// </summary>
        public List<string>? NewPhotoFilePaths { get; set; }

        /// <summary>
        /// Danh sách ID hình ảnh cần xóa
        /// </summary>
        public List<int>? DeletedPhotoIds { get; set; }
    }

    /// <summary>
    /// Additional Issue Photo DTO
    /// </summary>
    public class AdditionalIssuePhotoDto
    {
        public int Id { get; set; }
        public int AdditionalIssueId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
    }

    /// <summary>
    /// ✅ 2.3.3: DTO để tạo báo giá từ phát sinh
    /// </summary>
    public class CreateQuotationFromIssueDto
    {
        [Required(ErrorMessage = "Ít nhất một item là bắt buộc")]
        [MinLength(1, ErrorMessage = "Ít nhất một item là bắt buộc")]
        public List<CreateQuotationItemDto> Items { get; set; } = new();

        public DateTime? ValidUntil { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? Terms { get; set; }

        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [StringLength(1000)]
        public string? CustomerNotes { get; set; }
    }
}

