using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class ServiceOrderItem : BaseEntity
    {
        public int ServiceOrderId { get; set; }
        public int ServiceId { get; set; }
        
        [StringLength(200)]
        public string ServiceName { get; set; } = string.Empty; // Cached service name
        
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
        public string? Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed", "Cancelled"

        // ✅ THÊM: Phân công KTV và giờ công dự kiến cho item
        public int? AssignedTechnicianId { get; set; } // KTV được phân công cho item này
        public decimal? EstimatedHours { get; set; }   // Giờ công dự kiến cho item này

        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
        public virtual Employee? AssignedTechnician { get; set; } // Navigation to assigned technician
    }
}
