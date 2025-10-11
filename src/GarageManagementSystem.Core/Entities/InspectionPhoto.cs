using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Inspection Photo - Ảnh chụp khi kiểm tra
    /// </summary>
    public class InspectionPhoto : BaseEntity
    {
        public int VehicleInspectionId { get; set; }
        public int? InspectionIssueId { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(200)]
        public string? FileName { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } // Exterior, Interior, Engine, Issue, etc.

        [StringLength(500)]
        public string? Description { get; set; }

        public int? DisplayOrder { get; set; }

        // Navigation properties
        public virtual VehicleInspection VehicleInspection { get; set; } = null!;
        public virtual InspectionIssue? InspectionIssue { get; set; }
    }
}

