using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ 2.4.2: DTO cho Quality Control
    /// </summary>
    public class QualityControlDto : BaseDto
    {
        public int ServiceOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        
        public int? QCInspectorId { get; set; }
        public string? QCInspectorName { get; set; }
        
        public DateTime QCDate { get; set; }
        public string QCResult { get; set; } = "Pending"; // "Pending", "Pass", "Fail"
        public string? QCNotes { get; set; }
        
        public bool ReworkRequired { get; set; }
        public string? ReworkNotes { get; set; }
        
        public DateTime? QCCompletedDate { get; set; }
        
        public List<QCChecklistItemDto> QCChecklistItems { get; set; } = new();
    }

    /// <summary>
    /// ✅ 2.4.2: DTO cho QC Checklist Item
    /// </summary>
    public class QCChecklistItemDto : BaseDto
    {
        public int QualityControlId { get; set; }
        public string ChecklistItemName { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
        public string? Result { get; set; } // "Pass", "Fail", null
        public string? Notes { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// ✅ 2.4.2: DTO để tạo QC Inspection
    /// </summary>
    public class CreateQualityControlDto
    {
        [Required(ErrorMessage = "ServiceOrderId là bắt buộc")]
        public int ServiceOrderId { get; set; }
        
        public int? QCInspectorId { get; set; }
        
        public List<CreateQCChecklistItemDto> ChecklistItems { get; set; } = new();
    }

    /// <summary>
    /// ✅ 2.4.2: DTO để tạo QC Checklist Item
    /// </summary>
    public class CreateQCChecklistItemDto
    {
        [Required(ErrorMessage = "Tên checklist item là bắt buộc")]
        [StringLength(200)]
        public string ChecklistItemName { get; set; } = string.Empty;
        
        public bool IsChecked { get; set; }
        public string? Result { get; set; }
        public string? Notes { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// ✅ 2.4.2: DTO để hoàn thành QC với kết quả
    /// </summary>
    public class CompleteQCDto
    {
        [Required(ErrorMessage = "Kết quả QC là bắt buộc")]
        public string QCResult { get; set; } = string.Empty; // "Pass", "Fail"
        
        public string? QCNotes { get; set; }
        
        public bool ReworkRequired { get; set; }
        public string? ReworkNotes { get; set; }
        
        public List<CreateQCChecklistItemDto> ChecklistItems { get; set; } = new();
    }

    /// <summary>
    /// ✅ 2.4.4: DTO để bàn giao xe
    /// </summary>
    public class HandoverServiceOrderDto
    {
        [StringLength(200)]
        public string? HandoverLocation { get; set; }
        
        public DateTime? HandoverDate { get; set; }
        
        [StringLength(1000)]
        public string? HandoverNotes { get; set; }
    }

    /// <summary>
    /// ✅ 2.4.3: DTO để ghi nhận giờ công làm lại
    /// </summary>
    public class RecordReworkHoursDto
    {
        [Required(ErrorMessage = "Giờ công làm lại là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giờ công làm lại phải lớn hơn 0")]
        public decimal ReworkHours { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}

