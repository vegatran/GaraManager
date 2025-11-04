using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Additional Issue - Phát sinh phát hiện trong quá trình sửa chữa (khác với InspectionIssue - phát hiện lúc kiểm tra xe ban đầu)
    /// </summary>
    public class AdditionalIssue : BaseEntity
    {
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// ServiceOrderItem bị ảnh hưởng (item đang làm bị phát sinh)
        /// Nếu null, phát sinh ảnh hưởng toàn bộ ServiceOrder
        /// </summary>
        public int? ServiceOrderItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Engine, Brake, Suspension, Electrical, Body, Tire, etc.

        [Required]
        [StringLength(200)]
        public string IssueName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low

        public bool RequiresImmediateAction { get; set; } = false;

        /// <summary>
        /// KTV phát hiện phát sinh
        /// </summary>
        public int ReportedByEmployeeId { get; set; }

        [StringLength(1000)]
        public string? TechnicianNotes { get; set; }

        /// <summary>
        /// Trạng thái phát sinh: Identified (Mới phát hiện), Reported (Đã báo cáo), Quoted (Đã báo giá), Approved (Đã duyệt), Rejected (Từ chối), Repaired (Đã sửa)
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Identified"; // Identified, Reported, Quoted, Approved, Rejected, Repaired

        /// <summary>
        /// Thời gian phát hiện
        /// </summary>
        public DateTime ReportedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Liên kết đến ServiceQuotation bổ sung (nếu đã tạo báo giá)
        /// </summary>
        public int? AdditionalQuotationId { get; set; }

        /// <summary>
        /// Liên kết đến ServiceOrder bổ sung (LSC Bổ sung - nếu khách hàng đồng ý)
        /// </summary>
        public int? AdditionalServiceOrderId { get; set; }

        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual ServiceOrderItem? ServiceOrderItem { get; set; }
        public virtual Employee ReportedByEmployee { get; set; } = null!;
        public virtual ServiceQuotation? AdditionalQuotation { get; set; }
        public virtual ServiceOrder? AdditionalServiceOrder { get; set; }
        public virtual ICollection<AdditionalIssuePhoto> Photos { get; set; } = new List<AdditionalIssuePhoto>();
    }
}

