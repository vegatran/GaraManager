using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// LaborCategory - Danh mục công lao động
    /// </summary>
    public class LaborCategory : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty; // "Công tháo lắp", "Công sửa chữa", "Công sơn"
        
        [StringLength(20)]
        public string CategoryCode { get; set; } = string.Empty; // "REMOVE_INSTALL", "REPAIR", "PAINT"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public decimal BaseRate { get; set; } = 0; // Đơn giá cơ bản/giờ
        public decimal StandardRate { get; set; } = 0; // Alias for BaseRate
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<LaborItem> LaborItems { get; set; } = new List<LaborItem>();
    }
}
