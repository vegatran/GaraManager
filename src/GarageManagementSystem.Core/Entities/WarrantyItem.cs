using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Chi tiết bảo hành cho từng phụ tùng trong phiếu sửa chữa
    /// </summary>
    public class WarrantyItem : BaseEntity
    {
        public int WarrantyId { get; set; }
        public int? ServiceOrderPartId { get; set; }
        public int? PartId { get; set; }

        [StringLength(200)]
        public string PartName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? PartNumber { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        public int WarrantyMonths { get; set; } = 0;

        public DateTime WarrantyStartDate { get; set; }
        public DateTime WarrantyEndDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Expired, Voided

        [StringLength(500)]
        public string? Notes { get; set; }

        public virtual Warranty Warranty { get; set; } = null!;
        public virtual ServiceOrderPart? ServiceOrderPart { get; set; }
        public virtual Part? Part { get; set; }
    }
}

