namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ 2.3.4: DTO cho tiến độ Service Order
    /// </summary>
    public class ServiceOrderProgressDto
    {
        public int ServiceOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        
        // Progress Statistics
        public int TotalItems { get; set; }
        public int PendingItems { get; set; }
        public int InProgressItems { get; set; }
        public int CompletedItems { get; set; }
        public int OnHoldItems { get; set; }
        public int CancelledItems { get; set; }
        
        // Progress Percentage
        public decimal ProgressPercentage { get; set; } // 0-100
        
        // Time Statistics
        public decimal? TotalEstimatedHours { get; set; }
        public decimal? TotalActualHours { get; set; }
        public decimal? RemainingEstimatedHours { get; set; }
        
        // Status Timeline
        public DateTime? OrderDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        
        // Items với timeline
        public List<ServiceOrderItemProgressDto> Items { get; set; } = new();
    }
    
    /// <summary>
    /// ✅ 2.3.4: DTO cho tiến độ từng Service Order Item
    /// </summary>
    public class ServiceOrderItemProgressDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        // Time tracking
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        
        // Progress
        public decimal ProgressPercentage { get; set; } // 0-100
        
        // Assignment
        public int? AssignedTechnicianId { get; set; }
        public string? AssignedTechnicianName { get; set; }
    }
}

