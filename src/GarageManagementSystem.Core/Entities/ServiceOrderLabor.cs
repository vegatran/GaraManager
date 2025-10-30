using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ServiceOrderLabor - Công lao động trong đơn hàng
    /// </summary>
    public class ServiceOrderLabor : BaseEntity
    {
        public int ServiceOrderId { get; set; }
        public int LaborItemId { get; set; }
        public int? EmployeeId { get; set; } // Thợ thực hiện
        
        public decimal EstimatedHours { get; set; } = 0; // ✅ THÊM: Giờ công dự kiến
        [Required]
        public decimal ActualHours { get; set; } = 1; // Số giờ thực tế
        
        public decimal LaborRate { get; set; } = 0; // Đơn giá công/giờ
        
        [Required]
        public decimal TotalLaborCost { get; set; } = 0; // Tổng tiền công
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú thực hiện
        
        [StringLength(20)]
        public string? Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed"
        
        public DateTime? StartTime { get; set; } // Thời gian bắt đầu
        
        public DateTime? EndTime { get; set; } // Thời gian kết thúc
        
        public DateTime? CompletedTime { get; set; } // Thời gian hoàn thành
        
        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual LaborItem LaborItem { get; set; } = null!;
        public virtual Employee? Employee { get; set; }
    }
}
