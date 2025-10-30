using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO để phân công KTV cho một item trong ServiceOrder
    /// </summary>
    public class AssignTechnicianDto
    {
        [Required(ErrorMessage = "KTV là bắt buộc")]
        public int TechnicianId { get; set; }

        [Range(0.1, 24, ErrorMessage = "Giờ công dự kiến phải từ 0.1 đến 24 giờ")]
        public decimal? EstimatedHours { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO để chuyển trạng thái ServiceOrder
    /// </summary>
    public class ChangeServiceOrderStatusDto
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO để phân công hàng loạt cho tất cả items
    /// </summary>
    public class BulkAssignTechnicianDto
    {
        [Required(ErrorMessage = "KTV là bắt buộc")]
        public int TechnicianId { get; set; }

        [Range(0.1, 24, ErrorMessage = "Giờ công dự kiến phải từ 0.1 đến 24 giờ")]
        public decimal? EstimatedHours { get; set; }

        public List<int>? ItemIds { get; set; } // Nếu null thì áp dụng cho tất cả items
    }
}

