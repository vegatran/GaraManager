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
        
        // Phân loại nguồn gốc và hóa đơn
        [StringLength(30)]
        public string SourceType { get; set; } = "Purchased"; // Purchased, Used, Refurbished, Salvage
        
        [StringLength(50)]
        public string InvoiceType { get; set; } = "WithInvoice"; // WithInvoice, WithoutInvoice, Internal
        
        public bool HasInvoice { get; set; } = true; // Có hóa đơn hay không
        
        public bool CanUseForCompany { get; set; } = false; // Cho phép dùng cho công ty (cần hóa đơn)
        
        public bool CanUseForInsurance { get; set; } = false; // Cho phép dùng cho bảo hiểm (cần hóa đơn)
        
        public bool CanUseForIndividual { get; set; } = true; // Cho phép dùng cho cá nhân
        
        [StringLength(20)]
        public string Condition { get; set; } = "New"; // New, Used, Refurbished, AsIs
        
        [StringLength(100)]
        public string? SourceReference { get; set; } // Nguồn gốc: Tháo từ xe (biển số), Mua từ đâu
        
        // ✅ THÊM: Thông tin thuế VAT
        public decimal VATRate { get; set; } = 10; // Tỷ lệ thuế VAT mặc định (%)
        
        public bool IsVATApplicable { get; set; } = true; // Có áp dụng thuế VAT không

        // Thông tin kỹ thuật mở rộng
        public int? PartGroupId { get; set; } // Thuộc nhóm phụ tùng nào
        
        [StringLength(50)]
        public string? OEMNumber { get; set; } // Số phụ tùng chính hãng
        
        [StringLength(50)]
        public string? AftermarketNumber { get; set; } // Số phụ tùng thay thế
        
        [StringLength(100)]
        public string? Manufacturer { get; set; } // Nhà sản xuất phụ tùng
        
        [StringLength(100)]
        public string? Dimensions { get; set; } // "120x80x25mm"
        
        public decimal? Weight { get; set; } // Trọng lượng (kg)
        
        [StringLength(50)]
        public string? Material { get; set; } // "Nhựa", "Thép", "Nhôm"
        
        [StringLength(50)]
        public string? Color { get; set; } // "Đen", "Trắng", "Xám"
        
        public int WarrantyMonths { get; set; } = 0; // Bảo hành (tháng)
        
        [StringLength(100)]
        public string? WarrantyConditions { get; set; } // "Không bảo hành khi va chạm"
        
        public bool IsOEM { get; set; } = false; // Phụ tùng chính hãng

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual PartGroup? PartGroup { get; set; }
        public virtual ICollection<ServiceOrderPart> ServiceOrderParts { get; set; } = new List<ServiceOrderPart>();
        public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}

