using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// SupplierPerformance - Hiệu suất nhà cung cấp (tính toán)
    /// Phase 4.2.2 & 4.2.4: Đánh giá Nhà cung cấp & Hiệu suất Mua hàng
    /// </summary>
    public class SupplierPerformance : BaseEntity
    {
        public int SupplierId { get; set; }
        
        public int? PartId { get; set; } // Null = performance chung cho supplier
        
        [Required]
        public int TotalOrders { get; set; } = 0;
        
        [Required]
        public int OnTimeDeliveries { get; set; } = 0;
        
        [Required]
        public decimal OnTimeDeliveryRate { get; set; } = 0; // Percentage (0-100)
        
        [Required]
        public int AverageLeadTimeDays { get; set; } = 0;
        
        [Required]
        public decimal DefectRate { get; set; } = 0; // Percentage (0-100)
        
        [Required]
        public decimal AveragePrice { get; set; } = 0;
        
        [Required]
        public decimal PriceStability { get; set; } = 0; // Coefficient of variation
        
        [Required]
        public decimal OverallScore { get; set; } = 0; // 0-100
        
        [Required]
        public DateTime CalculatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Part? Part { get; set; }
    }
}

