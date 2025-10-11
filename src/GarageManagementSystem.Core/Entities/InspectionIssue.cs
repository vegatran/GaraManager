using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Inspection Issue - Vấn đề phát hiện khi kiểm tra
    /// </summary>
    public class InspectionIssue : BaseEntity
    {
        public int VehicleInspectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty; // Engine, Brake, Suspension, Electrical, Body, Tire, etc.

        [Required]
        [StringLength(200)]
        public string IssueName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium"; // Critical, High, Medium, Low

        public bool RequiresImmediateAction { get; set; } = false;

        public decimal? EstimatedCost { get; set; }

        [StringLength(1000)]
        public string? TechnicianNotes { get; set; }

        // Suggested service to fix this issue
        public int? SuggestedServiceId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Identified"; // Identified, Quoted, Approved, Repaired, Ignored

        // Navigation properties
        public virtual VehicleInspection VehicleInspection { get; set; } = null!;
        public virtual Service? SuggestedService { get; set; }
    }
}

