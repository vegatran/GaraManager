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

        // Vehicle Type Classification
        [StringLength(20)]
        public string VehicleType { get; set; } = "Personal"; // Personal, Insurance, Company

        // Insurance Information (nullable)
        [StringLength(100)]
        public string? InsuranceCompany { get; set; }

        [StringLength(50)]
        public string? PolicyNumber { get; set; }

        [StringLength(50)]
        public string? CoverageType { get; set; } // Full, Third Party, Comprehensive

        [StringLength(50)]
        public string? ClaimNumber { get; set; }

        [StringLength(100)]
        public string? AdjusterName { get; set; }

        [StringLength(20)]
        public string? AdjusterPhone { get; set; }

        // Company Information (nullable)
        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(20)]
        public string? TaxCode { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(50)]
        public string? CostCenter { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
        public virtual ICollection<VehicleInspection> Inspections { get; set; } = new List<VehicleInspection>();
        public virtual ICollection<ServiceQuotation> Quotations { get; set; } = new List<ServiceQuotation>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
