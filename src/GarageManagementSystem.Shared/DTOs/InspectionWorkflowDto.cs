namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO for inspection workflow recommendations
    /// </summary>
    public class InspectionWorkflowRecommendationsDto
    {
        public int InspectionId { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public string InspectionType { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public List<string> NextRecommendedSteps { get; set; } = new();
        public List<string> RequiredApprovals { get; set; } = new();
        public bool IsWorkflowBlocked { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    /// <summary>
    /// DTO for inspection type statistics
    /// </summary>
    public class InspectionTypeStatsDto
    {
        public int GeneralCount { get; set; }
        public int InsuranceCount { get; set; }
        public int CompanyCount { get; set; }
        public int DiagnosticCount { get; set; }
        public int PreServiceCount { get; set; }
        public int PostRepairCount { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<string, decimal> TypePercentages { get; set; } = new();
    }
}
