using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// DTO cho Financial Transaction
    /// </summary>
    public class FinancialTransactionDto
    {
        public int Id { get; set; }
        
        public string TransactionNumber { get; set; } = string.Empty;
        
        public string TransactionType { get; set; } = string.Empty; // "Income", "Expense"
        
        public string Category { get; set; } = string.Empty;
        
        public string? SubCategory { get; set; }
        
        public decimal Amount { get; set; }
        
        public string Currency { get; set; } = "VND";
        
        public DateTime TransactionDate { get; set; }
        
        public string PaymentMethod { get; set; } = string.Empty;
        
        public string? ReferenceNumber { get; set; }
        
        public string? Description { get; set; }
        
        public string? RelatedEntity { get; set; }
        
        public int? RelatedEntityId { get; set; }
        
        public int? EmployeeId { get; set; }
        
        public string? EmployeeName { get; set; }
        
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovedDate { get; set; }
        
        public string? Notes { get; set; }
        
        public bool IsApproved { get; set; }
        
        public bool IsReconciled { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO để tạo Financial Transaction mới
    /// </summary>
    public class CreateFinancialTransactionDto
    {
        [Required]
        public string TransactionType { get; set; } = string.Empty;
        
        [Required]
        public string Category { get; set; } = string.Empty;
        
        public string? SubCategory { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        public string Currency { get; set; } = "VND";
        
        [Required]
        public DateTime TransactionDate { get; set; }
        
        public string PaymentMethod { get; set; } = string.Empty;
        
        public string? ReferenceNumber { get; set; }
        
        public string? Description { get; set; }
        
        public string? RelatedEntity { get; set; }
        
        public int? RelatedEntityId { get; set; }
        
        public int? EmployeeId { get; set; }
        
        public string? Notes { get; set; }
    }
}

