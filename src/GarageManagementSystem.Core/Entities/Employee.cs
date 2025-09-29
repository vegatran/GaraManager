using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class Employee : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Position { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        public DateTime? HireDate { get; set; }

        public decimal? Salary { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } = "Active"; // Active, Inactive, Terminated

        [StringLength(1000)]
        public string? Skills { get; set; }

        // Navigation properties
        public virtual ICollection<ServiceOrder> AssignedServiceOrders { get; set; } = new List<ServiceOrder>();
    }
}
