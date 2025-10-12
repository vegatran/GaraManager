using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// VehicleBrand - Hãng xe
    /// </summary>
    public class VehicleBrand : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string BrandName { get; set; } = string.Empty; // "Mercedes-Benz", "BMW", "Toyota"
        
        [StringLength(20)]
        public string BrandCode { get; set; } = string.Empty; // "MB", "BMW", "TOY"
        
        [StringLength(100)]
        public string? Country { get; set; } // "Germany", "Japan"
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(500)]
        public string? LogoUrl { get; set; } // URL to brand logo image
        
        [StringLength(200)]
        public string? Website { get; set; } // Official website
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<VehicleModel> Models { get; set; } = new List<VehicleModel>();
        public virtual ICollection<PartGroupCompatibility> Compatibilities { get; set; } = new List<PartGroupCompatibility>();
    }
}
