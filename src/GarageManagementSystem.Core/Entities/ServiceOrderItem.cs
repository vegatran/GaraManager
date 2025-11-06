using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class ServiceOrderItem : BaseEntity
    {
        public int ServiceOrderId { get; set; }
        public int? ServiceId { get; set; } // ✅ SỬA: Cho phép null cho labor items (tiền công)
        
        [StringLength(200)]
        public string ServiceName { get; set; } = string.Empty; // Cached service name (cho labor items, dùng ItemName)
        
        [StringLength(500)]
        public string? Description { get; set; } // Mô tả chi tiết

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal UnitPrice { get; set; }
        
        public decimal Discount { get; set; } = 0; // Giảm giá
        
        public decimal FinalPrice { get; set; } = 0; // Giá sau giảm giá

        [Required]
        public decimal TotalPrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed", "Cancelled", "OnHold", "WaitingForCustomerApproval"

        // ✅ THÊM: Phân công KTV và giờ công dự kiến cho item
        public int? AssignedTechnicianId { get; set; } // KTV được phân công cho item này
        public decimal? EstimatedHours { get; set; }   // Giờ công dự kiến cho item này
        
        // ✅ 2.3.1: Giờ công thực tế và thời gian bắt đầu/kết thúc
        public DateTime? StartTime { get; set; } // Thời gian bắt đầu làm việc thực tế
        public DateTime? EndTime { get; set; }   // Thời gian kết thúc làm việc thực tế
        public decimal? ActualHours { get; set; } // Giờ công thực tế (tính từ StartTime và EndTime hoặc nhập thủ công)
        public DateTime? CompletedTime { get; set; } // Thời gian hoàn thành item

        /// <summary>
        /// ✅ 2.4.3: Giờ công làm lại (nếu QC không đạt)
        /// </summary>
        public decimal? ReworkHours { get; set; }

        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual Service? Service { get; set; } // ✅ SỬA: Cho phép null cho labor items (tiền công)
        public virtual Employee? AssignedTechnician { get; set; } // Navigation to assigned technician
    }
}
