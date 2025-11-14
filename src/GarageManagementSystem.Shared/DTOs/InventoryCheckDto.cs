using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class InventoryCheckDto : BaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CheckDate { get; set; }
        public int? WarehouseId { get; set; }
        public int? WarehouseZoneId { get; set; }
        public int? WarehouseBinId { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? StartedByEmployeeId { get; set; }
        public int? CompletedByEmployeeId { get; set; }
        public string? Notes { get; set; }
        
        // Navigation properties (optional - để hiển thị thông tin chi tiết)
        public string? WarehouseName { get; set; }
        public string? WarehouseZoneName { get; set; }
        public string? WarehouseBinName { get; set; }
        public string? StartedByEmployeeName { get; set; }
        public string? CompletedByEmployeeName { get; set; }
        public List<InventoryCheckItemDto> Items { get; set; } = new List<InventoryCheckItemDto>();
    }

    public class InventoryCheckItemDto : BaseDto
    {
        public int InventoryCheckId { get; set; }
        public int PartId { get; set; }
        public int SystemQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int DiscrepancyQuantity { get; set; }
        public string? Notes { get; set; }
        public bool IsDiscrepancy { get; set; }
        public bool IsAdjusted { get; set; }
        
        // Navigation properties (optional - để hiển thị thông tin chi tiết)
        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
        public string? PartSku { get; set; }
        public string? PartBarcode { get; set; }
    }

    public class CreateInventoryCheckDto
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime CheckDate { get; set; }

        public int? WarehouseId { get; set; }
        public int? WarehouseZoneId { get; set; }
        public int? WarehouseBinId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Draft";

        public DateTime? StartedDate { get; set; }
        public int? StartedByEmployeeId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<CreateInventoryCheckItemDto> Items { get; set; } = new List<CreateInventoryCheckItemDto>();
    }

    public class UpdateInventoryCheckDto : CreateInventoryCheckDto
    {
        [Required]
        public int Id { get; set; }

        public DateTime? CompletedDate { get; set; }
        public int? CompletedByEmployeeId { get; set; }
    }

    public class CreateInventoryCheckItemDto
    {
        [Required]
        public int PartId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int SystemQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ActualQuantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateInventoryCheckItemDto : CreateInventoryCheckItemDto
    {
        [Required]
        public int Id { get; set; }
    }
}

