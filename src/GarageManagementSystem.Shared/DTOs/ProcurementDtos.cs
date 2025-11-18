namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ Phase 4.2.1: Procurement Management DTOs
    /// </summary>
    
    public class DemandAnalysisDto
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int SuggestedQuantity { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int? SourceEntityId { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public DateTime SuggestedDate { get; set; }
    }

    public class ReorderSuggestionDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int SuggestedQuantity { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int? SourceEntityId { get; set; }
        public DateTime SuggestedDate { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public bool IsProcessed { get; set; }
        public int? PurchaseOrderId { get; set; }
    }

    public class BulkCreatePODto
    {
        public List<BulkCreatePOSuggestionDto> Suggestions { get; set; } = new();
        public int SupplierId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkCreatePOSuggestionDto
    {
        public int? SuggestionId { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public int SupplierId { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
    }

    /// <summary>
    /// ✅ Phase 4.2.2: Supplier Comparison DTOs
    /// </summary>
    public class SupplierComparisonDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public decimal UnitPrice { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public int LeadTimeDays { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public decimal DefectRate { get; set; }
        public decimal OverallScore { get; set; }
        public decimal CalculatedScore { get; set; }
        public bool IsPreferred { get; set; }
        public string? QuotationNumber { get; set; }
        public DateTime? QuotationDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? WarrantyPeriod { get; set; }
        public string? Notes { get; set; }
    }

    public class SupplierRecommendationDto
    {
        public int PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public int RecommendedQuantity { get; set; }
        public SupplierComparisonDto RecommendedSupplier { get; set; } = null!;
        public List<SupplierComparisonDto> AllSuppliers { get; set; } = new();
        public decimal AveragePrice { get; set; }
        public decimal PriceDifference { get; set; }
        public decimal PriceDifferencePercent { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;
        public DateTime RecommendedAt { get; set; }
    }

    /// <summary>
    /// ✅ Phase 4.2.4: Performance Evaluation DTOs
    /// </summary>
    public class SupplierPerformanceReportDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public int OnTimeDeliveries { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public int AverageLeadTimeDays { get; set; }
        public decimal DefectRate { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal PriceStability { get; set; }
        public decimal OverallScore { get; set; }
        public DateTime CalculatedAt { get; set; }
        public int? PartId { get; set; }
        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
    }

    public class SupplierRankingDto
    {
        public int Rank { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierCode { get; set; } = string.Empty;
        public decimal OverallScore { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public decimal DefectRate { get; set; }
        public decimal AveragePrice { get; set; }
        public int AverageLeadTimeDays { get; set; }
        public int TotalOrders { get; set; }
        public string PerformanceCategory { get; set; } = string.Empty; // "Excellent", "Good", "Average", "Poor"
    }

    public class PerformanceAlertDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "LowOnTimeDelivery", "HighDefectRate", "LowScore"
        public string AlertMessage { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal ThresholdValue { get; set; }
        public string Severity { get; set; } = string.Empty; // "High", "Medium", "Low"
        public DateTime AlertDate { get; set; }
    }

    public class CalculatePerformanceRequestDto
    {
        public int? SupplierId { get; set; }
        public int? PartId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ForceRecalculate { get; set; } = false;
    }
}

