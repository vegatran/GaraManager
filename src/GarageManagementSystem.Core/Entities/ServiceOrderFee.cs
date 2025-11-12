using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Chi tiết phí theo phiếu sửa chữa, liên kết với ServiceFeeType
    /// </summary>
    public class ServiceOrderFee : BaseEntity
    {
        public int ServiceOrderId { get; set; }

        public int ServiceFeeTypeId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public decimal VatAmount { get; set; } = 0;

        public decimal DiscountAmount { get; set; } = 0;

        [StringLength(200)]
        public string? ReferenceSource { get; set; } // Ví dụ: "ServiceOrderItem:123"

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsManual { get; set; } = false;

        public virtual ServiceOrder ServiceOrder { get; set; } = null!;

        public virtual ServiceFeeType ServiceFeeType { get; set; } = null!;
    }
}

