using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PartGroup - Nhóm phụ tùng chung
    /// </summary>
    public class PartGroup : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string GroupName { get; set; } = string.Empty; // "Bộ lọc gió động cơ"
        
        [StringLength(50)]
        public string GroupCode { get; set; } = string.Empty; // "AIR_FILTER_ENGINE"
        
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // "Engine System"
        
        [StringLength(100)]
        public string SubCategory { get; set; } = string.Empty; // "Air Intake"
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Function { get; set; } // "Lọc không khí vào động cơ"
        
        [StringLength(50)]
        public string? Unit { get; set; } // "Cái", "Bộ", "Lít"
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
        public virtual ICollection<PartGroupCompatibility> Compatibilities { get; set; } = new List<PartGroupCompatibility>();
        public virtual ICollection<LaborItem> LaborItems { get; set; } = new List<LaborItem>();
    }
}
