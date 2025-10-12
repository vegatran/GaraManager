using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(20)]
    public string? AlternativePhone { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(50)]
    public string? TaxCode { get; set; } // Company tax code

    [StringLength(100)]
    public string? ContactPersonName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
        public virtual ICollection<VehicleInspection> VehicleInspections { get; set; } = new List<VehicleInspection>();
        public virtual ICollection<ServiceQuotation> ServiceQuotations { get; set; } = new List<ServiceQuotation>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
