using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// Payment - Thanh toán cho Invoice (hỗ trợ partial payment)
    /// </summary>
    public class Payment : BaseEntity
    {
        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }

        [StringLength(255)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        public int? InvoiceId { get; set; }

        [StringLength(50)]
        public string? InvoiceNumber { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Bank Transfer, Credit Card, E-Wallet, QR Code, Other

        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // Mã giao dịch, số chuyển khoản

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Completed"; // Completed, Cancelled, Pending

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Invoice? Invoice { get; set; }
    }
}

