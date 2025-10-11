using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Part - Phụ tùng/Vật tư
    /// </summary>
    public class Part : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; } = string.Empty; // Mã phụ tùng

        [Required]
        [StringLength(200)]
        public string PartName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } // Dầu, Lốp, Má phanh, Lọc...

        [StringLength(100)]
        public string? Brand { get; set; } // Hãng phụ tùng

        [Required]
        public decimal CostPrice { get; set; } = 0; // Giá nhập

        [Required]
        public decimal AverageCostPrice { get; set; } = 0; // Giá nhập trung bình (cho tính lãi/lỗ)

        [Required]
        public decimal SellPrice { get; set; } = 0; // Giá bán

        [Required]
        public int QuantityInStock { get; set; } = 0; // Tồn kho hiện tại

        public int MinimumStock { get; set; } = 0; // Cảnh báo tồn kho tối thiểu

        public int? ReorderLevel { get; set; } // Mức đặt hàng lại

        [StringLength(20)]
        public string? Unit { get; set; } // Đơn vị: cái, lít, bộ, kg...

        [StringLength(500)]
        public string? CompatibleVehicles { get; set; } // Xe nào dùng được (Brand/Model)

        [StringLength(100)]
        public string? Location { get; set; } // Vị trí trong kho

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<ServiceOrderPart> ServiceOrderParts { get; set; } = new List<ServiceOrderPart>();
        public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}

