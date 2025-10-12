using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// VehicleModel - Model xe
    /// </summary>
    public class VehicleModel : BaseEntity
    {
        public int BrandId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ModelName { get; set; } = string.Empty; // "C-Class", "3 Series", "Camry"
        
        [StringLength(20)]
        public string ModelCode { get; set; } = string.Empty; // "W205", "G20", "XV70"
        
        [StringLength(20)]
        public string? Generation { get; set; } // "W205", "W206"
        
        [StringLength(10)]
        public string? StartYear { get; set; } // "2014"
        
        [StringLength(10)]
        public string? EndYear { get; set; } // "2021"
        
        [StringLength(50)]
        public string? VehicleType { get; set; } // "Sedan", "SUV", "Coupe"
        
        [StringLength(50)]
        public string? Segment { get; set; } // "Compact", "Mid-size", "Luxury"
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual VehicleBrand Brand { get; set; } = null!;
        public virtual ICollection<PartGroupCompatibility> Compatibilities { get; set; } = new List<PartGroupCompatibility>();
        public virtual ICollection<EngineSpecification> EngineSpecifications { get; set; } = new List<EngineSpecification>();
    }
}
