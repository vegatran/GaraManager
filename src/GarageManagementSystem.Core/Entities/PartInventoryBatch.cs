using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PartInventoryBatch - Lô hàng phụ tùng (để phân biệt hàng có/không hóa đơn)
    /// </summary>
    public class PartInventoryBatch : BaseEntity
    {
        public int PartId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty; // Mã lô hàng
        
        [Required]
        public DateTime ReceiveDate { get; set; } = DateTime.Now; // Ngày nhập kho
        
        [Required]
        public int QuantityReceived { get; set; } // Số lượng nhập
        
        public int QuantityRemaining { get; set; } // Số lượng còn lại
        
        [Required]
        public decimal UnitCost { get; set; } // Giá nhập đơn vị
        
        // Phân loại nguồn gốc
        [Required]
        [StringLength(30)]
        public string SourceType { get; set; } = "Purchased"; // Purchased, Used, Refurbished, Salvage
        
        [StringLength(20)]
        public string Condition { get; set; } = "New"; // New, Used, Refurbished, AsIs
        
        // Thông tin hóa đơn
        public bool HasInvoice { get; set; } = true; // Có hóa đơn hay không
        
        [StringLength(50)]
        public string? InvoiceNumber { get; set; } // Số hóa đơn
        
        public DateTime? InvoiceDate { get; set; } // Ngày hóa đơn
        
        [StringLength(100)]
        public string? SupplierName { get; set; } // Nhà cung cấp
        
        public int? SupplierId { get; set; } // ID nhà cung cấp
        
        // Phân loại sử dụng
        public bool CanUseForCompany { get; set; } = false; // Dùng cho công ty (cần hóa đơn)
        
        public bool CanUseForInsurance { get; set; } = false; // Dùng cho bảo hiểm (cần hóa đơn)
        
        public bool CanUseForIndividual { get; set; } = true; // Dùng cho cá nhân
        
        // Nguồn gốc chi tiết
        [StringLength(100)]
        public string? SourceReference { get; set; } // "Tháo từ xe 30A-12345", "Mua lẻ tại chợ"
        
        [StringLength(100)]
        public string? SourceVehicle { get; set; } // Biển số xe (nếu tháo từ xe cũ)
        
        public int? SourceVehicleId { get; set; } // ID xe (nếu tháo từ xe cũ)
        
        public int? SourceServiceOrderId { get; set; } // Đơn hàng nguồn (nếu tháo từ sửa chữa)
        
        // Thông tin lưu trữ
        [StringLength(100)]
        public string? Location { get; set; } // Vị trí trong kho
        
        [StringLength(50)]
        public string? Shelf { get; set; } // Kệ
        
        [StringLength(50)]
        public string? Bin { get; set; } // Ngăn

        public int? WarehouseId { get; set; }
        public int? WarehouseZoneId { get; set; }
        public int? WarehouseBinId { get; set; }
        
        // Ngày hết hạn (nếu có)
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsExpired { get; set; } = false;
        
        // Ghi chú
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(100)]
        public string? ReceivedBy { get; set; } // Người nhận hàng
        
        public int? EmployeeId { get; set; } // ID nhân viên nhận
        
        public bool IsActive { get; set; } = true; // Còn tồn kho
        
        // Navigation properties
        public virtual Part Part { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual Vehicle? SourceVehicleEntity { get; set; }
        public virtual ServiceOrder? SourceServiceOrder { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<PartBatchUsage> BatchUsages { get; set; } = new List<PartBatchUsage>();
        public virtual Warehouse? Warehouse { get; set; }
        public virtual WarehouseZone? WarehouseZone { get; set; }
        public virtual WarehouseBin? WarehouseBin { get; set; }
    }
}
