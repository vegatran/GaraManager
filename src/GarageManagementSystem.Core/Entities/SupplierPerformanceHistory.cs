using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// SupplierPerformanceHistory - Lịch sử hiệu suất nhà cung cấp
    /// Phase 4.2.4: Đánh giá Hiệu suất Mua hàng
    /// </summary>
    public class SupplierPerformanceHistory : BaseEntity
    {
        public int SupplierId { get; set; }
        
        public int? PartId { get; set; }
        
        [Required]
        public DateTime PeriodStart { get; set; }
        
        [Required]
        public DateTime PeriodEnd { get; set; }
        
        [Required]
        public decimal OnTimeDeliveryRate { get; set; } = 0;
        
        [Required]
        public decimal AverageDelayDays { get; set; } = 0;
        
        [Required]
        public decimal DefectRate { get; set; } = 0;
        
        [Required]
        public decimal OverallScore { get; set; } = 0;
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Part? Part { get; set; }
    }
}

