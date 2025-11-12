using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Đơn khiếu nại bảo hành
    /// </summary>
    public class WarrantyClaim : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string ClaimNumber { get; set; } = string.Empty;

        public int WarrantyId { get; set; }
        public int? ServiceOrderId { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }

        public DateTime ClaimDate { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string IssueDescription { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed

        [StringLength(1000)]
        public string? Resolution { get; set; }

        public DateTime? ResolvedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public virtual Warranty Warranty { get; set; } = null!;
        public virtual ServiceOrder? ServiceOrder { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
    }
}

