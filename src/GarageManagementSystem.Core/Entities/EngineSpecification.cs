using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// EngineSpecification - Thông số động cơ
    /// </summary>
    public class EngineSpecification : BaseEntity
    {
        public int ModelId { get; set; }
        
        [StringLength(50)]
        public string EngineCode { get; set; } = string.Empty; // "M274", "B48", "2AR-FE"
        
        [StringLength(100)]
        public string EngineName { get; set; } = string.Empty; // "2.0L Turbo", "1.6L Hybrid"
        
        [StringLength(20)]
        public string Displacement { get; set; } = string.Empty; // "1991cc", "1598cc"
        
        [StringLength(20)]
        public string FuelType { get; set; } = string.Empty; // "Gasoline", "Diesel", "Hybrid"
        
        [StringLength(20)]
        public string Aspiration { get; set; } = string.Empty; // "Turbo", "Supercharged", "Natural"
        
        [StringLength(20)]
        public string CylinderLayout { get; set; } = string.Empty; // "I4", "V6", "V8"
        
        public int CylinderCount { get; set; } = 4; // Số xi lanh
        
        [StringLength(20)]
        public string? StartYear { get; set; }
        
        [StringLength(20)]
        public string? EndYear { get; set; }
        
        // Navigation properties
        public virtual VehicleModel Model { get; set; } = null!;
        public virtual ICollection<PartGroupCompatibility> Compatibilities { get; set; } = new List<PartGroupCompatibility>();
    }
}
