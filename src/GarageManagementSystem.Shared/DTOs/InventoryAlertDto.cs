using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class InventoryAlertDto : BaseDto
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // LowStock, OutOfStock, Expired, NearExpiry
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public string Message { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int? MinimumQuantity { get; set; }
        public int? ReorderQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime AlertDate { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class CreateInventoryAlertDto
    {
        [Required]
        public int PartId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string AlertType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        public int CurrentQuantity { get; set; }
        public int? MinimumQuantity { get; set; }
        public int? ReorderQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class UpdateInventoryAlertDto : CreateInventoryAlertDto
    {
        [Required]
        public int Id { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedBy { get; set; }
    }

    public class ResolveAlertDto
    {
        [StringLength(1000)]
        public string? ResolutionNotes { get; set; }
    }
}
