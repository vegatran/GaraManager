using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Vị trí cụ thể (kệ/ngăn) trong kho
    /// </summary>
    public class WarehouseBin : BaseEntity
    {
        [Required]
        public int WarehouseId { get; set; }

        public int? WarehouseZoneId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Capacity { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual WarehouseZone? WarehouseZone { get; set; }
        public virtual ICollection<PartInventoryBatch> InventoryBatches { get; set; } = new List<PartInventoryBatch>();
    }
}

