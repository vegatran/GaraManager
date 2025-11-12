using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Warranty entity lưu thông tin bảo hành cho phiếu sửa chữa
    /// </summary>
    public class Warranty : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string WarrantyCode { get; set; } = string.Empty;

        public int ServiceOrderId { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }

        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Expired, Voided

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(50)]
        public string? HandoverBy { get; set; }

        [StringLength(200)]
        public string? HandoverLocation { get; set; }

        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;

        public virtual ICollection<WarrantyItem> Items { get; set; } = new List<WarrantyItem>();
        public virtual ICollection<WarrantyClaim> Claims { get; set; } = new List<WarrantyClaim>();
    }
}

