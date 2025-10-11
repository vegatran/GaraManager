using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// PaymentTransaction - Giao dịch thanh toán (cho phép thanh toán nhiều lần)
    /// </summary>
    public class PaymentTransaction : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty; // Số biên lai: PT-20241006-0001

        public int ServiceOrderId { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        public decimal Amount { get; set; } // Số tiền thanh toán lần này

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Transfer, E-Wallet, Check

        [StringLength(100)]
        public string? TransactionReference { get; set; } // Mã giao dịch (chuyển khoản, thẻ...)

        [StringLength(100)]
        public string? CardType { get; set; } // Visa, MasterCard... (nếu thanh toán thẻ)

        [StringLength(4)]
        public string? CardLastFourDigits { get; set; } // 4 số cuối thẻ

        public int? ReceivedById { get; set; } // Employee thu tiền

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsRefund { get; set; } = false; // Có phải là hoàn tiền không

        [StringLength(500)]
        public string? RefundReason { get; set; }

        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual Employee? ReceivedBy { get; set; }
    }
}

