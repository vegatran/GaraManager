using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ Phase 4.2.3: PO Tracking DTOs
    /// </summary>

    /// <summary>
    /// DTO cho thông tin tracking của PO
    /// </summary>
    public class PurchaseOrderTrackingDto
    {
        public int PurchaseOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? InTransitDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingMethod { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? DeliveryNotes { get; set; }
        public int? DaysUntilDelivery { get; set; }
        public List<PurchaseOrderStatusHistoryDto> StatusHistory { get; set; } = new();
    }

    /// <summary>
    /// DTO cho status history
    /// </summary>
    public class PurchaseOrderStatusHistoryDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StatusDate { get; set; }
        public string? Notes { get; set; }
        public int? UpdatedByEmployeeId { get; set; }
    }

    /// <summary>
    /// DTO để cập nhật tracking info
    /// </summary>
    public class UpdateTrackingDto
    {
        [StringLength(100)]
        public string? TrackingNumber { get; set; }
        
        [StringLength(100)]
        public string? ShippingMethod { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [StringLength(500)]
        public string? DeliveryNotes { get; set; }
        
        public bool? MarkAsInTransit { get; set; }
        public DateTime? InTransitDate { get; set; }
    }

    /// <summary>
    /// DTO để mark PO as InTransit
    /// </summary>
    public class MarkInTransitDto
    {
        public DateTime? InTransitDate { get; set; }
        
        [StringLength(100)]
        public string? TrackingNumber { get; set; }
        
        [StringLength(100)]
        public string? ShippingMethod { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho delivery alerts
    /// </summary>
    public class DeliveryAlertsDto
    {
        public int AtRiskCount { get; set; }
        public int DelayedCount { get; set; }
        public int TotalCount { get; set; }
        public List<PurchaseOrderDto> AtRiskOrders { get; set; } = new();
        public List<PurchaseOrderDto> DelayedOrders { get; set; } = new();
    }
}

