using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    public class Employee : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        // Foreign keys for Department and Position
        public int? PositionId { get; set; }
        public int? DepartmentId { get; set; }

        // Legacy string fields (tạm thời giữ để không break existing data)
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
        public virtual Position? PositionNavigation { get; set; }
        public virtual Department? DepartmentNavigation { get; set; }
        public virtual ICollection<ServiceOrder> AssignedServiceOrders { get; set; } = new List<ServiceOrder>();
        public virtual ICollection<VehicleInspection> PerformedInspections { get; set; } = new List<VehicleInspection>();
        public virtual ICollection<ServiceQuotation> PreparedQuotations { get; set; } = new List<ServiceQuotation>();
        public virtual ICollection<PaymentTransaction> ReceivedPayments { get; set; } = new List<PaymentTransaction>();
        public virtual ICollection<StockTransaction> ProcessedStockTransactions { get; set; } = new List<StockTransaction>();
        public virtual ICollection<Appointment> AssignedAppointments { get; set; } = new List<Appointment>();
        public virtual ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();
    }
}
