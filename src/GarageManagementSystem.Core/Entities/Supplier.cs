using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Supplier - Nhà cung cấp phụ tùng
    /// </summary>
    public class Supplier : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string SupplierCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        [StringLength(50)]
        public string? TaxCode { get; set; } // Mã số thuế

        [StringLength(100)]
        public string? BankAccount { get; set; }

        [StringLength(200)]
        public string? BankName { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Rating (1-5 stars)
        public int? Rating { get; set; }

        // Navigation properties
        public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}

