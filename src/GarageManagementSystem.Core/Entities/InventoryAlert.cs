using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageManagementSystem.Core.Entities
{
    [Table("InventoryAlerts")]
    public class InventoryAlert : BaseEntity
    {
        [Required]
        public int PartId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string AlertType { get; set; } = string.Empty; // LowStock, OutOfStock, Expired, NearExpiry
        
        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        public int CurrentQuantity { get; set; }
        public int? MinimumQuantity { get; set; }
        public int? ReorderQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime AlertDate { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }

        // Navigation properties
        [ForeignKey(nameof(PartId))]
        public virtual Part Part { get; set; } = null!;
    }
}
