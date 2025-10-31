using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceOrderItemDto : BaseDto
    {
        public int ServiceOrderId { get; set; }
        public int? ServiceId { get; set; } // ✅ SỬA: Cho phép null cho labor items
        public string ServiceName { get; set; } = string.Empty; // ✅ THÊM: Tên dịch vụ (cho labor items dùng ItemName)
        public ServiceDto? Service { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        
        // ✅ THÊM: Thông tin phân công KTV và giờ công dự kiến
        public int? AssignedTechnicianId { get; set; }
        public string? AssignedTechnicianName { get; set; }
        public decimal? EstimatedHours { get; set; }
    }

    public class CreateServiceOrderItemDto
    {
        public int? ServiceId { get; set; } // ✅ SỬA: Cho phép null cho labor items
        public string? ServiceName { get; set; } // ✅ THÊM: Tên dịch vụ (cho labor items)

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    public class UpdateServiceOrderItemDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}
