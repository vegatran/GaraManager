using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PartSupplier - Liên kết phụ tùng với nhà cung cấp (nhiều-nhiều)
    /// </summary>
    public class PartSupplier : BaseEntity
    {
        public int PartId { get; set; }
        
        public int SupplierId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SupplierPartNumber { get; set; } = string.Empty; // Mã phụ tùng của nhà cung cấp
        
        [Required]
        public decimal CostPrice { get; set; } // Giá nhập
        
        [StringLength(10)]
        public string Currency { get; set; } = "VND";
        
        public int MinimumOrderQuantity { get; set; } = 1; // Số lượng đặt tối thiểu
        
        public int LeadTimeDays { get; set; } = 7; // Thời gian giao hàng (ngày)
        
        [StringLength(100)]
        public string? Packaging { get; set; } // Đóng gói
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public bool IsPreferred { get; set; } = false; // Nhà cung cấp ưu tiên
        
        public DateTime? LastOrderDate { get; set; } // Ngày đặt hàng gần nhất
        
        public decimal? LastCostPrice { get; set; } // Giá nhập lần trước
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Part Part { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;
    }
}

