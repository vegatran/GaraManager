using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO for changing vehicle type
    /// </summary>
    public class ChangeVehicleTypeDto
    {
        [Required]
        [StringLength(20, ErrorMessage = "Vehicle type cannot exceed 20 characters")]
        public string VehicleType { get; set; } = "Personal";
    }

    /// <summary>
    /// DTO for vehicle workflow status
    /// </summary>
    public class VehicleWorkflowStatusDto
    {
        public int VehicleId { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public bool HasActiveInspection { get; set; }
        public bool HasPendingQuotation { get; set; }
        public bool HasApprovedQuotation { get; set; }
        public bool HasActiveServiceOrder { get; set; }
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? LastQuotationDate { get; set; }
        public DateTime? LastServiceOrderDate { get; set; }
        
        // Workflow recommendations
        public string CurrentWorkflowStep { get; set; } = string.Empty;
        public string NextRecommendedAction { get; set; } = string.Empty;
        public List<string> RequiredApprovals { get; set; } = new();
        public bool IsWorkflowBlocked { get; set; }
        public string? BlockingReason { get; set; }
    }

    /// <summary>
    /// DTO for vehicle type statistics
    /// </summary>
    public class VehicleTypeStatsDto
    {
        public int PersonalCount { get; set; }
        public int InsuranceCount { get; set; }
        public int CompanyCount { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<string, decimal> TypePercentages { get; set; } = new();
    }
}
