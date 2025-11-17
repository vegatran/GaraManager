using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    // Forward reference - CreateInventoryCheckItemDto được định nghĩa trong InventoryCheckDto.cs
    /// <summary>
    /// DTO cho bulk update Parts
    /// </summary>
    public class BulkUpdatePartsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 phụ tùng")]
        public List<int> PartIds { get; set; } = new();

        // Các fields có thể update hàng loạt
        public int? MinimumStock { get; set; }
        public int? ReorderLevel { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public int? WarehouseId { get; set; }
        public int? WarehouseZoneId { get; set; }
        public int? WarehouseBinId { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO cho bulk delete Parts
    /// </summary>
    public class BulkDeletePartsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 phụ tùng")]
        public List<int> PartIds { get; set; } = new();
    }

    /// <summary>
    /// DTO cho bulk update Inventory Check Items
    /// </summary>
    public class BulkUpdateInventoryCheckItemsDto
    {
        [Required]
        public int InventoryCheckId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 item")]
        public List<int> ItemIds { get; set; } = new();

        public int? ActualQuantity { get; set; }
        public string? Notes { get; set; }
        public bool? IsDiscrepancy { get; set; }
    }

    /// <summary>
    /// DTO cho bulk add Inventory Check Items
    /// </summary>
    public class BulkAddInventoryCheckItemsDto
    {
        [Required]
        public int InventoryCheckId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 item")]
        public List<CreateInventoryCheckItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO cho bulk update Warehouses/Zones/Bins
    /// </summary>
    public class BulkUpdateWarehousesDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 kho")]
        public List<int> WarehouseIds { get; set; } = new();

        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class BulkUpdateWarehouseZonesDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 khu vực")]
        public List<int> ZoneIds { get; set; } = new();

        public bool? IsActive { get; set; }
    }

    public class BulkUpdateWarehouseBinsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 kệ")]
        public List<int> BinIds { get; set; } = new();

        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }

    /// <summary>
    /// DTO cho bulk approve Inventory Adjustments
    /// </summary>
    public class BulkApproveInventoryAdjustmentsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 phiếu điều chỉnh")]
        public List<int> AdjustmentIds { get; set; } = new();

        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cho bulk reject Inventory Adjustments
    /// </summary>
    public class BulkRejectInventoryAdjustmentsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 phiếu điều chỉnh")]
        public List<int> AdjustmentIds { get; set; } = new();

        [Required]
        public string RejectionReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO cho bulk operations
    /// </summary>
    public class BulkOperationResultDto
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<int> SuccessIds { get; set; } = new();
        public List<int> FailedIds { get; set; } = new();
    }
}

