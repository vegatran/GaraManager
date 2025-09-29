using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class Vehicle : BaseEntity
    {
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Year { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(17)]
        public string? VIN { get; set; }

        [StringLength(50)]
        public string? EngineNumber { get; set; }

        public int CustomerId { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
    }
}
