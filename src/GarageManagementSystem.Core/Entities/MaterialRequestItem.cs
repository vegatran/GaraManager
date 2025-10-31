using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class MaterialRequestItem : BaseEntity
    {
        [Required]
        public int MaterialRequestId { get; set; }

        [Required]
        public int PartId { get; set; }

        [Required]
        public int QuantityRequested { get; set; }

        public int QuantityApproved { get; set; } = 0;
        public int QuantityPicked { get; set; } = 0;
        public int QuantityIssued { get; set; } = 0;
        public int QuantityDelivered { get; set; } = 0;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Snapshot info for printing
        [StringLength(200)]
        public string? PartName { get; set; }

        // Navigation
        public virtual MaterialRequest MaterialRequest { get; set; } = null!;
        public virtual Part Part { get; set; } = null!;
    }
}


