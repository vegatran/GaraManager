using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Supplier - Nhà cung cấp
    /// </summary>
    public class Supplier : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string SupplierName { get; set; } = string.Empty; // "Bosch", "Continental", "Valeo"
        
        [StringLength(20)]
        public string SupplierCode { get; set; } = string.Empty; // "BOS", "CON", "VAL"
        
        [StringLength(100)]
        public string? ContactPerson { get; set; } // Người liên hệ
        
        [StringLength(20)]
        public string? Phone { get; set; } // Số điện thoại
        
        [StringLength(20)]
        public string? ContactPhone { get; set; } // Alias for Phone (API compatibility)
        
        [StringLength(100)]
        public string? Email { get; set; } // Email
        
        [StringLength(200)]
        public string? Address { get; set; } // Địa chỉ
        
        [StringLength(50)]
        public string? City { get; set; } // Thành phố
        
        [StringLength(50)]
        public string? Country { get; set; } // Quốc gia
        
        [StringLength(100)]
        public string? Website { get; set; } // Website
        
        [StringLength(50)]
        public string? TaxCode { get; set; } // Mã số thuế
        
        [StringLength(100)]
        public string? BankAccount { get; set; } // Số tài khoản ngân hàng
        
        [StringLength(100)]
        public string? BankName { get; set; } // Tên ngân hàng
        
        [StringLength(50)]
        public string? PaymentTerms { get; set; } // "30 days", "COD", "Prepaid"
        
        [StringLength(100)]
        public string? DeliveryTerms { get; set; } // "FOB", "CIF", "EXW"
        
        [StringLength(500)]
        public string? Notes { get; set; } // Ghi chú
        
        public bool IsOEMSupplier { get; set; } = false; // Nhà cung cấp chính hãng
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? LastOrderDate { get; set; } // Ngày đặt hàng cuối
        
        public decimal? TotalOrderValue { get; set; } = 0; // Tổng giá trị đã đặt
        
        public decimal Rating { get; set; } = 5.0m; // Supplier rating (1-5 stars)
        
        // Navigation properties
        public virtual ICollection<PartSupplier> PartSuppliers { get; set; } = new List<PartSupplier>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}