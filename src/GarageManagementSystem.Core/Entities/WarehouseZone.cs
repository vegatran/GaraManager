using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Khu vực/thứ cấp trong kho
    /// </summary>
    public class WarehouseZone : BaseEntity
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<WarehouseBin> Bins { get; set; } = new List<WarehouseBin>();
        public virtual ICollection<PartInventoryBatch> InventoryBatches { get; set; } = new List<PartInventoryBatch>();
    }
}

