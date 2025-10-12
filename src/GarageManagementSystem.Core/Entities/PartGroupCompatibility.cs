using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PartGroupCompatibility - Tương thích nhóm phụ tùng
    /// </summary>
    public class PartGroupCompatibility : BaseEntity
    {
        public int PartGroupId { get; set; }
        public int? BrandId { get; set; } // null = tất cả hãng
        public int? ModelId { get; set; } // null = tất cả model của hãng
        public int? EngineSpecificationId { get; set; } // Thông số động cơ cụ thể
        
        [StringLength(50)]
        public string? BodyType { get; set; } // "Sedan", "SUV", "Coupe", "Convertible"
        
        [StringLength(50)]
        public string? DriveType { get; set; } // "FWD", "RWD", "AWD", "4WD"
        
        [StringLength(50)]
        public string? TransmissionType { get; set; } // "Manual", "Automatic", "CVT", "DCT"
        
        [StringLength(50)]
        public string? FuelSystem { get; set; } // "Direct Injection", "Port Injection", "Hybrid"
        
        [StringLength(10)]
        public string? StartYear { get; set; } // "2015"
        
        [StringLength(10)]
        public string? EndYear { get; set; } // "2020"
        
        [StringLength(100)]
        public string? SpecialNotes { get; set; } // "Chỉ dành cho xe có package Sport"
        
        public bool IsOEMOnly { get; set; } = false; // Chỉ dùng phụ tùng chính hãng
        
        public bool IsAftermarketAllowed { get; set; } = true; // Cho phép phụ tùng thay thế
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual PartGroup PartGroup { get; set; } = null!;
        public virtual VehicleBrand? Brand { get; set; }
        public virtual VehicleModel? Model { get; set; }
        public virtual EngineSpecification? EngineSpecification { get; set; }
    }
}
