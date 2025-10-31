using System.ComponentModel.DataAnnotations;
using GarageManagementSystem.Core.Enums;

namespace GarageManagementSystem.Core.Entities
{
    public class MaterialRequest : BaseEntity
    {
        [Required]
        public string MRNumber { get; set; } = string.Empty; // MR-yyyymmdd-####

        [Required]
        public int ServiceOrderId { get; set; }

        public int RequestedById { get; set; } // KTV
        public int? ApprovedById { get; set; } // Quản đốc/Tổ trưởng
        public int? IssuedById { get; set; }   // Thủ kho
        public DateTime? ApprovedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [Required]
        public MaterialRequestStatus Status { get; set; } = MaterialRequestStatus.Draft;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? RejectReason { get; set; }

        // Navigation
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual ICollection<MaterialRequestItem> Items { get; set; } = new List<MaterialRequestItem>();
    }
}


