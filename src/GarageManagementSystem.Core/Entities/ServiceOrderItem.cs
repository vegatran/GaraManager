using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class ServiceOrderItem : BaseEntity
    {
        public int ServiceOrderId { get; set; }
        public int ServiceId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
