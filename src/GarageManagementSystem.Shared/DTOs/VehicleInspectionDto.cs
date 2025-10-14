using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class VehicleInspectionDto : BaseDto
    {
        public string InspectionNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public int? InspectorId { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionType { get; set; } = string.Empty;
        public int? CurrentMileage { get; set; }
        public string? FuelLevel { get; set; }
        public string? GeneralCondition { get; set; }
        public string? ExteriorCondition { get; set; }
        public string? InteriorCondition { get; set; }
        public string? EngineCondition { get; set; }
        public string? BrakeCondition { get; set; }
        public string? SuspensionCondition { get; set; }
        public string? TireCondition { get; set; }
        public string? ElectricalCondition { get; set; }
        public string? LightsCondition { get; set; }
        public string? CustomerComplaints { get; set; }
        public string? Recommendations { get; set; }
        public string? TechnicianNotes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public int? QuotationId { get; set; }
        
        // Navigation properties
        public CustomerDto? Customer { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public EmployeeDto? Inspector { get; set; }
        public List<InspectionIssueDto> Issues { get; set; } = new();
        public List<InspectionPhotoDto> Photos { get; set; } = new();
    }

    public class CreateVehicleInspectionDto
    {
        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        public int? InspectorId { get; set; }

        public DateTime InspectionDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string InspectionType { get; set; } = "General";

        public int? CurrentMileage { get; set; }

        [StringLength(20)]
        public string? FuelLevel { get; set; }

        [StringLength(2000)]
        public string? GeneralCondition { get; set; }

        [StringLength(5000)]
        public string? ExteriorCondition { get; set; }

        [StringLength(5000)]
        public string? InteriorCondition { get; set; }

        [StringLength(5000)]
        public string? EngineCondition { get; set; }

        [StringLength(5000)]
        public string? BrakeCondition { get; set; }

        [StringLength(5000)]
        public string? SuspensionCondition { get; set; }

        [StringLength(5000)]
        public string? TireCondition { get; set; }

        [StringLength(5000)]
        public string? ElectricalCondition { get; set; }

        [StringLength(5000)]
        public string? LightsCondition { get; set; }

        [StringLength(2000)]
        public string? CustomerComplaints { get; set; }

        [StringLength(2000)]
        public string? Recommendations { get; set; }

        [StringLength(2000)]
        public string? TechnicianNotes { get; set; }

        public List<CreateInspectionIssueDto> Issues { get; set; } = new();
    }

    public class UpdateVehicleInspectionDto : CreateVehicleInspectionDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        public DateTime? CompletedDate { get; set; }
    }

    public class InspectionIssueDto : BaseDto
    {
        public int VehicleInspectionId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string IssueName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool RequiresImmediateAction { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? TechnicianNotes { get; set; }
        public int? SuggestedServiceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public ServiceDto? SuggestedService { get; set; }
    }

    public class CreateInspectionIssueDto
    {
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string IssueName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium";

        public bool RequiresImmediateAction { get; set; } = false;

        public decimal? EstimatedCost { get; set; }

        [StringLength(5000)]
        public string? TechnicianNotes { get; set; }

        public int? SuggestedServiceId { get; set; }
    }

    public class InspectionPhotoDto
    {
        public int Id { get; set; }
        public int VehicleInspectionId { get; set; }
        public int? InspectionIssueId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
    }
}

