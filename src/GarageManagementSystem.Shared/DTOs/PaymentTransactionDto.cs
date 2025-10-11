using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    public class PaymentTransactionDto : BaseDto
    {
        public string ReceiptNumber { get; set; } = string.Empty;
        public int ServiceOrderId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }
        public string? CardType { get; set; }
        public string? CardLastFourDigits { get; set; }
        public int? ReceivedById { get; set; }
        public string? Notes { get; set; }
        public bool IsRefund { get; set; }
        public string? RefundReason { get; set; }
        public EmployeeDto? ReceivedBy { get; set; }
    }

    public class CreatePaymentTransactionDto
    {
        [Required] public int ServiceOrderId { get; set; }
        [Required] [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
        [Required] [StringLength(50)] public string PaymentMethod { get; set; } = "Cash";
        [StringLength(100)] public string? TransactionReference { get; set; }
        [StringLength(100)] public string? CardType { get; set; }
        [StringLength(4)] public string? CardLastFourDigits { get; set; }
        public int? ReceivedById { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
        public bool IsRefund { get; set; } = false;
        [StringLength(500)] public string? RefundReason { get; set; }
    }
}

