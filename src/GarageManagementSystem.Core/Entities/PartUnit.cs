using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Đơn vị quy đổi cho phụ tùng
    /// </summary>
    public class PartUnit : BaseEntity
    {
        [Required]
        public int PartId { get; set; }

        [Required]
        [StringLength(50)]
        public string UnitName { get; set; } = string.Empty;

        /// <summary>
        /// Hệ số quy đổi so với đơn vị chuẩn (DefaultUnit). Ví dụ: Thùng (10), Lít (1).
        /// </summary>
        [Range(0.0001, double.MaxValue)]
        public decimal ConversionRate { get; set; } = 1m;

        /// <summary>
        /// Barcode riêng cho đơn vị này (nếu có).
        /// </summary>
        [StringLength(150)]
        public string? Barcode { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public bool IsDefault { get; set; } = false;

        public virtual Part Part { get; set; } = null!;
    }
}

