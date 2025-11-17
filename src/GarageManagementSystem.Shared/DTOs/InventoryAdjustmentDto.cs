using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class InventoryAdjustmentDto : BaseDto
    {
        public string AdjustmentNumber { get; set; } = string.Empty;
        public int? InventoryCheckId { get; set; }
        public string? InventoryCheckCode { get; set; }
        public string? InventoryCheckName { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int? WarehouseZoneId { get; set; }
        public string? WarehouseZoneName { get; set; }
        public int? WarehouseBinId { get; set; }
        public string? WarehouseBinName { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? Reason { get; set; }
        public int? ApprovedByEmployeeId { get; set; }
        public string? ApprovedByEmployeeName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? Notes { get; set; }
        public List<InventoryAdjustmentItemDto> Items { get; set; } = new();
    }

    public class InventoryAdjustmentItemDto : BaseDto
    {
        public int InventoryAdjustmentId { get; set; }
        public int PartId { get; set; }
        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
        public string? PartSku { get; set; }
        public int? InventoryCheckItemId { get; set; }
        public int QuantityChange { get; set; } // Có thể âm
        public int SystemQuantityBefore { get; set; }
        public int SystemQuantityAfter { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateInventoryAdjustmentDto
    {
        public int? InventoryCheckId { get; set; }
        public int? WarehouseId { get; set; }
        public int? WarehouseZoneId { get; set; }
        public int? WarehouseBinId { get; set; }
        public DateTime AdjustmentDate { get; set; } = DateTime.Now;
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public List<CreateInventoryAdjustmentItemDto> Items { get; set; } = new();
    }

    public class CreateInventoryAdjustmentItemDto
    {
        [Required]
        public int PartId { get; set; }
        public int? InventoryCheckItemId { get; set; }
        [Required]
        public int QuantityChange { get; set; }
        [Required]
        public int SystemQuantityBefore { get; set; }
        [Required]
        public int SystemQuantityAfter { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateInventoryAdjustmentDto
    {
        [Required]
        public int Id { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class ApproveInventoryAdjustmentDto
    {
        public string? Notes { get; set; }
    }

    public class RejectInventoryAdjustmentDto
    {
        [Required]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class CreateInventoryAdjustmentFromCheckDto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}


