using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// SupplierRating - Đánh giá nhà cung cấp
    /// Phase 4.2.2: Đánh giá Nhà cung cấp
    /// </summary>
    public class SupplierRating : BaseEntity
    {
        public int SupplierId { get; set; }
        
        public int? PartId { get; set; } // Null = rating chung cho supplier
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } = 5; // 1-5 stars
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        [Required]
        public int RatedByEmployeeId { get; set; }
        
        [Required]
        public DateTime RatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual Part? Part { get; set; }
        public virtual Employee RatedByEmployee { get; set; } = null!;
    }
}

