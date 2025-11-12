using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Kho vật tư
    /// </summary>
    public class Warehouse : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? ManagerName { get; set; }

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<WarehouseZone> Zones { get; set; } = new List<WarehouseZone>();
        public virtual ICollection<WarehouseBin> Bins { get; set; } = new List<WarehouseBin>();
        public virtual ICollection<PartInventoryBatch> InventoryBatches { get; set; } = new List<PartInventoryBatch>();
    }
}

